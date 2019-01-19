using Game.Utilities;
using UnityEditor;
using UnityEngine;

namespace Game.Deforming
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
            
            Material selectedBrushMaterial = surfaceData.SurfacePaints[selectedPaintIndex].BrushMaterial;
            RenderTexture selectedErasedAlphaMap = surfaceData.SurfacePaints[selectedPaintIndex].AlphaMap;
                
            DrawOnAlphaMap(textureCoord, ref selectedBrushMaterial, ref selectedErasedAlphaMap, 1);

            surfaceData.SurfacePaints[selectedPaintIndex].BrushMaterial = selectedBrushMaterial;
            surfaceData.SurfacePaints[selectedPaintIndex].AlphaMap = selectedErasedAlphaMap;

            for (int i = 0; i < surfaceData.SurfacePaints.Length; i++)
            {
                if (i == selectedPaintIndex)
                {
                    continue;
                }
                
                Material brushMaterial = surfaceData.SurfacePaints[i].BrushMaterial;
                RenderTexture erasedAlphaMap = surfaceData.SurfacePaints[i].AlphaMap;
                
                DrawOnAlphaMap(textureCoord, ref brushMaterial, ref erasedAlphaMap, 0);

                surfaceData.SurfacePaints[i].BrushMaterial = brushMaterial;
                surfaceData.SurfacePaints[i].AlphaMap = erasedAlphaMap;
            }
        }

        private void DrawDeform(Vector2 textureCoord)
        {
            Material brushMaterial = surfaceData.DeformPaint.BrushMaterial;
            RenderTexture alphaMap = surfaceData.DeformPaint.AlphaMap;
            
            DrawOnAlphaMap(textureCoord, ref brushMaterial, ref alphaMap, 1);

            surfaceData.DeformPaint.BrushMaterial = brushMaterial;
            surfaceData.DeformPaint.AlphaMap = alphaMap;
        }

        private void EraseSurfaces(Vector2 textureCoord)
        {
            for (int i = 0; i < surfaceData.SurfacePaints.Length; i++)
            {
                Material brushMaterial = surfaceData.SurfacePaints[i].BrushMaterial;
                RenderTexture erasedAlphaMap = surfaceData.SurfacePaints[i].AlphaMap;
                
                DrawOnAlphaMap(textureCoord, ref brushMaterial, ref erasedAlphaMap, 0);

                surfaceData.SurfacePaints[i].BrushMaterial = brushMaterial;
                surfaceData.SurfacePaints[i].AlphaMap = erasedAlphaMap;
            }
        }

        private void EraseDeform(Vector2 textureCoord)
        {
            Material brushMaterial = surfaceData.DeformPaint.BrushMaterial;
            RenderTexture alphaMap = surfaceData.DeformPaint.AlphaMap;
            
            DrawOnAlphaMap(textureCoord, ref brushMaterial, ref alphaMap, 0);

            surfaceData.DeformPaint.BrushMaterial = brushMaterial;
            surfaceData.DeformPaint.AlphaMap = alphaMap;
        }
        
        private void DrawOnAlphaMap(Vector2 textureCoord, ref Material brushMaterial, ref RenderTexture alphaMap, int appends = 1)
        {
            if (brushMaterial == null || alphaMap == null)
            {
                return;
            }
            
            brushMaterial.SetFloat("_Appends", appends);
            brushMaterial.SetVector("_Coordinate", new Vector4(textureCoord.x, textureCoord.y, 0f, 0f));
            brushMaterial.SetFloat("_Strength", brushOpacity);
            brushMaterial.SetFloat("_Size", brushSize);

            RenderTexture tempAlphaMap =
                RenderTexture.GetTemporary(alphaMap.width, alphaMap.height, alphaMap.depth, alphaMap.format);
            
            Graphics.Blit(alphaMap, tempAlphaMap);
            Graphics.Blit(tempAlphaMap, alphaMap, brushMaterial);
            
            RenderTexture.ReleaseTemporary(tempAlphaMap);
        }

        public void Initialize()
        {
            // SurfaceData is created before this method is called...
            
            surfaceData.CreatePaintedSurfaceMaterial();
            surfaceData.CreateDeformPaint();
            surfaceData.CreateSurfacePaints();
            
            surfaceData.UpdateMainTextures();
            surfaceData.UpdateDeformTextures();
            surfaceData.UpdateSurfaceTextures(true);
        }

        public void Load()
        {
            UpdateMaterial();
        }

        public void UpdateMaterial()
        {
            MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
            meshRenderer.material = surfaceData.PaintedSurfaceMaterial;
        }

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