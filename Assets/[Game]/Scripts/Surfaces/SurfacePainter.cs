using UnityEngine;

namespace Game.Surfaces
{
    public class SurfacePainter : MonoBehaviour
    {
        public const int MAX_PAINTS = 5;
        
        [SerializeField] private float brushSize = 10;

        [SerializeField] private float brushOpacity = 0.25f;

        [SerializeField] private int paintType;

        [SerializeField] private int selectedPaintIndex = -1;

        [SerializeField] private int visibleSurfacePaintCount;

        [SerializeField] private SurfaceData surfaceData;

        private void Awake()
        {
            Load();
        }

        private void Update()
        {
            Camera camera = FindObjectOfType<Camera>();

            if (camera == null)
            {
                return;
            }
            
            surfaceData.PaintedSurfaceMaterial.SetMatrix("_World2Camera", camera.worldToCameraMatrix);
        }

        private void DrawSurface(Vector2 textureCoord)
        {
            if (selectedPaintIndex == -1)
            {
                return;
            }
            
            if (selectedPaintIndex > surfaceData.SurfacePaints.Length - 1)
            {
                selectedPaintIndex = surfaceData.SurfacePaints.Length - 1;
            }
            
            RenderTexture selectedAlphaMap = surfaceData.SurfacePaints[selectedPaintIndex].AlphaMap;
                
            DrawOnAlphaMap(textureCoord, selectedAlphaMap, 1);

            for (int i = 0; i < surfaceData.SurfacePaints.Length; i++)
            {
                if (i == selectedPaintIndex)
                {
                    continue;
                }
                
                RenderTexture erasedAlphaMap = surfaceData.SurfacePaints[i].AlphaMap;
                
                DrawOnAlphaMap(textureCoord, erasedAlphaMap, 0);
            }
        }

        private void DrawDeform(Vector2 textureCoord)
        {
            DrawOnAlphaMap(textureCoord, surfaceData.DeformPaint.AlphaMap, 1);
        }

        private void EraseSurfaces(Vector2 textureCoord)
        {
            for (int i = 0; i < surfaceData.SurfacePaints.Length; i++)
            {
                RenderTexture erasedAlphaMap = surfaceData.SurfacePaints[i].AlphaMap;
                
                DrawOnAlphaMap(textureCoord, erasedAlphaMap, 0);
            }
        }

        private void EraseDeform(Vector2 textureCoord)
        {
            DrawOnAlphaMap(textureCoord, surfaceData.DeformPaint.AlphaMap, 0);
        }
        
        private void DrawOnAlphaMap(Vector2 textureCoord, RenderTexture alphaMap, int appends = 1)
        {
            if (surfaceData.BrushMaterial == null || alphaMap == null)
            {
                return;
            }
            
            surfaceData.BrushMaterial.SetFloat("_Appends", appends);
            surfaceData.BrushMaterial.SetVector("_Coordinate", new Vector4(textureCoord.x, textureCoord.y, 0f, 0f));
            surfaceData.BrushMaterial.SetFloat("_Strength", brushOpacity);
            surfaceData.BrushMaterial.SetFloat("_Size", brushSize);

            RenderTexture tempAlphaMap =
                RenderTexture.GetTemporary(alphaMap.width, alphaMap.height, alphaMap.depth, alphaMap.format);

            RenderTexture.active = tempAlphaMap;
            
            Graphics.Blit(alphaMap, tempAlphaMap);
            Graphics.Blit(tempAlphaMap, alphaMap, surfaceData.BrushMaterial);

            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(tempAlphaMap);
        }

#if UNITY_EDITOR
        public void Initialize()
        {
            // SurfaceData is created before this method is called...
            
            surfaceData.CreateBrushMaterial();
            surfaceData.CreatePaintedSurfaceMaterial();
            surfaceData.CreateDeformPaint();
            surfaceData.CreateSurfacePaints();
            surfaceData.CreateAlphaMapDataHolders();
            
            surfaceData.UpdateMainTextures();
            surfaceData.UpdateDeformTextures();
            surfaceData.UpdateSurfaceTextures(true);
        }

        public void Save()
        {
            surfaceData.Save();
        }
#endif

        public void Load()
        {
            if (surfaceData == null || surfaceData.PaintedSurfaceMaterial == null)
            {
                return;
            }
            
            UpdateMaterial();
            
            surfaceData.Load();
        }

        public void UpdateMaterial()
        {
            MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
            meshRenderer.material = surfaceData.PaintedSurfaceMaterial;
        }

#if UNITY_EDITOR
        public void UpdateMainTextures()
        {
            surfaceData.UpdateMainTextures();
        }

        public void UpdateDeformTextures()
        {
            surfaceData.UpdateDeformTextures();
        }

        public void UpdateSurfaceTextures()
        {
            surfaceData.UpdateSurfaceTextures();
        }
#endif

        public void Draw(Vector2 textureCoord)
        {
            if (paintType == 0)
            {
                DrawSurface(textureCoord);
            }
            else
            {
                DrawDeform(textureCoord);
            }
        }

        public void EraseDeform(Vector2 textureCoord, float brushSize, float brushOpacity)
        {
            float prevBrushSize = this.brushSize;
            float prevBrushOpacity = this.brushOpacity;
            
            this.brushSize = brushSize;
            this.brushOpacity = brushOpacity;
            
            EraseDeform(textureCoord);

            this.brushSize = prevBrushSize;
            this.brushOpacity = prevBrushOpacity;
        }

        public void Erase(Vector2 textureCoord)
        {
            if (paintType == 0)
            {
                EraseSurfaces(textureCoord);
            }
            else
            {
                EraseDeform(textureCoord);
            }
        }
    }
}