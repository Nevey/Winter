using System.Linq;
using UnityEngine;

namespace DS
{
    public class SurfaceData : ScriptableObject
    {
        #region Const
        public static class ShaderProperties
        {
            public const string SH_PATH_DEPTH_TEX = "Hidden/Deformable Surface/Depth Texture";
            public const string SH_PATH_STANDARD = "Deformable Surface/Deformable Surface Standard";
            public const string SH_PATH_STANDARD_FADE = "Deformable Surface/Deformable Surface Standard Fade";

            public static readonly string[] TEXTURES =
            {
                "_MainTex", "_albedo_flat", "_BumpMap", "_normal_flat"
            };

            public const string BASE_MAP = "_baseMap";
            public const string SRC_MAP = "_srcMap";

            public const string HEIGHT_MAX = "_height_max";
            public const string OFFSET_MAX = "_offset_max";
            public const string BLEND_H = "_blend_height";
            public const string TESS_AMOUNT = "_tess_amount";
            public const string TESS_DIST = "_tess_distance";
        }

        public const int MIN_MESH_SIZE = 8;
        public const int MAX_MESH_SIZE = 248;
        public const float MIN_MESH_SCALE = 0.05f;
        public const int MAX_HEIGHTMAP_RESOLUTION = 16;
        public const float MAX_DEFORM_SPEED = 1;
        public const int MAX_TESS_QUALITY = 7;
        public const int MAX_SMOOTH_RADIUS = 6;
        public const int MAX_SMOOTH_PASSES = 10;
        #endregion
        
        #region Geometry
        public int MeshWidth
        {
            get { return _meshWidth; }
            set
            {
                if (value == _meshWidth || Application.isPlaying)
                    return;

                value -= value % MIN_MESH_SIZE;
                _meshWidth = Mathf.Clamp(value, MIN_MESH_SIZE, MAX_MESH_SIZE);

                BuildBaseMap();
                BuildMeshes();
            }
        }
        [SerializeField]
        private int _meshWidth = MIN_MESH_SIZE * 4;

        public int MeshLength
        {
            get { return _meshLength; }
            set
            {
                if (value == _meshLength || Application.isPlaying)
                    return;

                value -= value % MIN_MESH_SIZE;
                _meshLength = Mathf.Clamp(value, MIN_MESH_SIZE, MAX_MESH_SIZE);

                BuildBaseMap();
                BuildMeshes();
            }
        }
        [SerializeField]
        private int _meshLength = MIN_MESH_SIZE * 4;

        public float MeshScale
        {
            get { return _meshScale; }
            set
            {
                if (Mathf.Approximately(value, _meshScale) || Application.isPlaying)
                    return;

                _meshScale = Mathf.Clamp(value, MIN_MESH_SCALE, float.MaxValue);
                BuildMeshes();
            }
        }
        [SerializeField]
        private float _meshScale = 1;
        #endregion

        #region Maps
        public int MapsResolution
        {
            get { return _mapsResolution; }
            set
            {
                if (value == _mapsResolution || Application.isPlaying)
                    return;

                _mapsResolution = Mathf.Clamp(value, 1, MAX_HEIGHTMAP_RESOLUTION);

                BuildBaseMap();
                BuildMeshes();
            }
        }
        [SerializeField] private int _mapsResolution = 4;

        public float MaxHeight
        {
            get { return _maxHeight; }
            set
            {
                if (Application.isPlaying)
                    return;

                _maxHeight = Mathf.Clamp(value, 1, float.MaxValue);
                BuildMeshes();
                UpdateMaterial();
                RebuildNormals();
            }
        }
        [SerializeField] private float _maxHeight = 10f;

        public float MaxOffset
        {
            get { return _maxOffset; }
            set
            {
                if (Application.isPlaying)
                    return;

                _maxOffset = Mathf.Clamp(value, 0.01f, float.MaxValue);
                UpdateMaterial();
                UpdateMeshBounds();
                RebuildNormals();
            }
        }
        [SerializeField] private float _maxOffset = 0.5f;
        #endregion

        #region Textures
        public float NormalIntensity
        {
            get { return _normalIntensity; }
            set
            {
                if (Application.isPlaying)
                    return;

                _normalIntensity = Mathf.Clamp01(value);
                RebuildNormals();
            }
        }
        [SerializeField] private float _normalIntensity = 0.75f;

        public float TextureSize
        {
            get { return _textureSize; }
            set
            {
                if (Mathf.Approximately(value, _textureSize) || Application.isPlaying)
                    return;

                Vector2 tiling = new Vector2(1 / value, 1 / value);

                foreach (var t in ShaderProperties.TEXTURES)
                {
                    if (SourceMaterial.HasProperty(t))
                        SourceMaterial.SetTextureScale(t, tiling);
                }

                _textureSize = value;
            }
        }
        [SerializeField] private float _textureSize = 1;

        public float TextureBlendHeight
        {
            get { return _blendHeight; }
            set
            {
                if (Application.isPlaying)
                    return;

                _blendHeight = Mathf.Clamp(value, 0.01f, float.MaxValue);
                UpdateMaterial();
            }
        }
        [SerializeField] private float _blendHeight = 1;
        #endregion

        #region Deformation
        public LayerMask DeformMask
        {
            get { return _deformMask; }
            set { _deformMask = value; }
        }
        [SerializeField] private LayerMask _deformMask = ~0;

        public float DeformSpeed
        {
            get { return _deformSpeed; }
            set { _deformSpeed = Mathf.Clamp(value, 0, MAX_DEFORM_SPEED); }
        }
        [SerializeField] private float _deformSpeed = 0.25f;

        public bool Smoothing = true;

        public int SmoothingRadius
        {
            get { return _smoothingRadius; }
            set { _smoothingRadius = Mathf.Clamp(value, 0, MAX_SMOOTH_RADIUS); }
        }
        [SerializeField] private int _smoothingRadius = 6;

        public int SmoothingPasses
        {
            get { return _smoothingPasses; }
            set { _smoothingPasses = Mathf.Clamp(value, 1, MAX_SMOOTH_PASSES); }
        }
        [SerializeField] private int _smoothingPasses = 2;
        #endregion

        #region Tess
        public int TessQuality
        {
            get { return _tessQuality; }
            set
            {
                _tessQuality = Mathf.Clamp(value, 1, MAX_TESS_QUALITY);
                UpdateMaterial();
            }
        }
        [SerializeField] private int _tessQuality = 3;

        public float TessDistance
        {
            get { return _tessDistance; }
            set
            {
                _tessDistance = Mathf.Clamp(value, 0, float.MaxValue);
                UpdateMaterial();
            }
        }
        [SerializeField] private float _tessDistance = 100;
        #endregion

        #region Asset sources
        public Mesh MeshBase;
        public Mesh MeshCollider;

        public Texture2D BaseMap;

        public Shader DepthTextureShader
        {
            get
            {
                return _depthTextureShader == null
                    ? _depthTextureShader = Shader.Find("Deformable Surface/Depth Texture")
                    : _depthTextureShader;
            }
            set { _depthTextureShader = value; }
        }
        [SerializeField] private Shader _depthTextureShader;

        public ComputeShader SurfaceKernels;
        public Material SourceMaterial;

        #endregion
        

        public void BuildMeshes()
        {
            SurfaceMeshParams p = new SurfaceMeshParams(MeshWidth, MeshLength, MeshScale);

            MeshBase.Clear();
            MeshBase.vertices = SurfaceMeshBuilder.GetVertices(p, BaseMap, MaxHeight, MapsResolution);
            MeshBase.SetIndices(SurfaceMeshBuilder.GetIndicesQuad(p), MeshTopology.Quads, 0);
            MeshBase.uv = SurfaceMeshBuilder.GetUV1(p);
            MeshBase.uv2 = SurfaceMeshBuilder.GetUV2(p);
            
            MeshBase.RecalculateNormals();
            MeshBase.RecalculateTangents();

            MeshCollider.Clear();
            MeshCollider.vertices = MeshBase.vertices;
            MeshCollider.triangles = SurfaceMeshBuilder.GetTriangles(p);
            MeshCollider.uv = MeshBase.uv2;

            MeshCollider.RecalculateNormals();

            UpdateMeshBounds();
        }

        public void BuildBaseMap()
        {
            int w = MeshWidth * MapsResolution;
            int h = MeshLength * MapsResolution;

            BaseMap.Resize(w, h);
            
            Color[] colors = new Color[w * h];
            int colId = 0;
            for (int i = 0; i < h; i++)
            {
                for (int j = 0; j < w; j++)
                {
                    if (i != 0 && i != h - 1 && j != 0 && j != w - 1)
                        colors[colId].g = 1;
                    colId++;
                }
            }

            BaseMap.SetPixels(colors);
            BaseMap.Apply();

            UpdateMaterial();
            RebuildNormals();
            BuildMeshes();
        }

        public void RebuildNormals()
        {
            int w = BaseMap.width;
            int h = BaseMap.height;

            RenderTexture baseMapTemp = new RenderTexture(w, h, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
            {
                enableRandomWrite = true
            };
            baseMapTemp.Create();
            Graphics.Blit(BaseMap, baseMapTemp);
            
            SurfaceKernels.SetTexture(DeformableSurface.DispatchKernels.GEN_NORMAL, "BaseMap", baseMapTemp);
            SurfaceKernels.SetFloat("NormalIntensity", NormalIntensity);
            SurfaceKernels.SetFloats("RtDimensions", BaseMap.width - 1, BaseMap.height - 1);

            SurfaceKernels.Dispatch(DeformableSurface.DispatchKernels.GEN_NORMAL,
                w / DeformableSurface.DispatchKernels.GROUP_THREADS_COUNT,
                h / DeformableSurface.DispatchKernels.GROUP_THREADS_COUNT,
                1);

            RenderTexture.active = baseMapTemp;

            BaseMap.ReadPixels(new Rect(0, 0, baseMapTemp.width, baseMapTemp.height), 0, 0);
            BaseMap.Apply();

            RenderTexture.active = null;
            DestroyImmediate(baseMapTemp);
        }


        public void UpdateMaterial()
        {
            if (SourceMaterial.HasProperty(ShaderProperties.HEIGHT_MAX))
                SourceMaterial.SetFloat(ShaderProperties.HEIGHT_MAX, _maxHeight);

            if (SourceMaterial.HasProperty(ShaderProperties.OFFSET_MAX))
                SourceMaterial.SetFloat(ShaderProperties.OFFSET_MAX, _maxOffset);

            if (SourceMaterial.HasProperty(ShaderProperties.BLEND_H))
                SourceMaterial.SetFloat(ShaderProperties.BLEND_H, _blendHeight);

            if (SourceMaterial.HasProperty(ShaderProperties.TESS_AMOUNT))
                SourceMaterial.SetFloat(ShaderProperties.TESS_AMOUNT, _tessQuality + (_tessQuality - 1));

            if (SourceMaterial.HasProperty(ShaderProperties.TESS_DIST))
                SourceMaterial.SetFloat(ShaderProperties.TESS_DIST, _tessDistance);
        }

        public void UpdateMeshBounds()
        {
            MeshBase.bounds = SurfaceMeshBuilder.GetBounds(
                new SurfaceMeshParams(MeshWidth, MeshLength, MeshScale),
                MaxHeight + MaxOffset);
        }

        public void FlattenMap(SurfaceMapType mapType, float value)
        {
            value = Mathf.Clamp(value, 0, float.MaxValue);
            
            for (int i = 0; i < BaseMap.width; i++)
            {
                for (int j = 0; j < BaseMap.height; j++)
                {
                    Color c = BaseMap.GetPixel(i, j);
                    if (mapType == SurfaceMapType.Height)
                        c.r = value;
                    else c.g = value;

                    BaseMap.SetPixel(i, j, c);
                }
            }

            BaseMap.Apply();

            if (mapType == SurfaceMapType.Height)
            {
                BuildMeshes();
                UpdateMeshBounds();
            }

            RebuildNormals();
        }

        public void RandomizeMap(SurfaceMapType mapType, float size, float amplitude, int seed)
        {
            float max = mapType == SurfaceMapType.Height ? MaxHeight : MaxOffset;

            int w = BaseMap.width;
            int h = BaseMap.height;

            Color[] colors = BaseMap.GetPixels(0, 0, w, h);
            int colId = 0;
            float noiseMul = Mathf.Clamp(size, -0.99f, 0.99f) / MapsResolution;
            for (int i = 0; i < h; i++)
            {
                for (int j = 0; j < w; j++)
                {
                    float v = Mathf.PerlinNoise(j * noiseMul + seed, i * noiseMul + seed) * amplitude / max;
                    if (mapType == SurfaceMapType.Height)
                        colors[colId].r = v;
                    else colors[colId].g = v;

                    colId++;
                }
            }
            BaseMap.SetPixels(colors);
            BaseMap.Apply();

            if (mapType == SurfaceMapType.Height)
            {
                BuildMeshes();
                UpdateMeshBounds();
            }

            RebuildNormals();
        }
    }

    public enum SurfaceMapType
    {
        Height = 0, Offset
    }

    public enum SurfaceShaderType
    {
        Standard, StandardFade, Custom
    }
}

