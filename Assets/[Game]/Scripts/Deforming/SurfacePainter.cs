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

        [SerializeField] private SurfaceData surfaceData;

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
            
            RenderTexture paintedAlphaMap = surfaceData.SurfacePaints[selectedPaintIndex].AlphaMap;
            DrawOnAlphaMap(textureCoord, ref paintedAlphaMap, 1);
            surfaceData.SurfacePaints[selectedPaintIndex].AlphaMap = paintedAlphaMap;

            for (int i = 0; i < surfaceData.SurfacePaints.Length; i++)
            {
                if (i == selectedPaintIndex)
                {
                    continue;
                }
                
                RenderTexture erasedAlphaMap = surfaceData.SurfacePaints[i].AlphaMap;
                DrawOnAlphaMap(textureCoord, ref erasedAlphaMap, 0);
                surfaceData.SurfacePaints[i].AlphaMap = erasedAlphaMap;
            }
        }

        private void DrawDeform(Vector2 textureCoord)
        {
            RenderTexture alphaMap = surfaceData.DeformPaint.AlphaMap;
            DrawOnAlphaMap(textureCoord, ref alphaMap, 1);
            surfaceData.DeformPaint.AlphaMap = alphaMap;
        }

        private void EraseSurfaces(Vector2 textureCoord)
        {
            for (int i = 0; i < surfaceData.SurfacePaints.Length; i++)
            {
                RenderTexture erasedAlphaMap = surfaceData.SurfacePaints[i].AlphaMap;
                DrawOnAlphaMap(textureCoord, ref erasedAlphaMap, 0);
                surfaceData.SurfacePaints[i].AlphaMap = erasedAlphaMap;
            }
        }

        private void EraseDeform(Vector2 textureCoord)
        {
            Material brushMaterial = surfaceData.BrushMaterial;
            RenderTexture alphaMap = surfaceData.DeformPaint.AlphaMap;
            
            DrawOnAlphaMap(textureCoord, ref alphaMap, 0);

            surfaceData.BrushMaterial = brushMaterial;
            surfaceData.DeformPaint.AlphaMap = alphaMap;
        }
        
        private void DrawOnAlphaMap(Vector2 textureCoord, ref RenderTexture alphaMap, int appends = 1)
        {
            if (surfaceData.BrushMaterial == null || alphaMap == null)
            {
                return;
            }
            
            surfaceData.BrushMaterial.SetFloat("_Appends", appends);
            surfaceData.BrushMaterial.SetVector("_Coordinate", new Vector4(textureCoord.x, textureCoord.y, 0f, 0f));
            surfaceData.BrushMaterial.SetFloat("_Strength", brushOpacity);
            surfaceData.BrushMaterial.SetFloat("_Size", brushSize);
            
            RenderTexture tempSplatMap = RenderTexture.GetTemporary(alphaMap.width, alphaMap.height, 0,
                RenderTextureFormat.ARGB32);
            
            Graphics.Blit(alphaMap, tempSplatMap);
            Graphics.Blit(tempSplatMap, alphaMap, surfaceData.BrushMaterial);
            
            RenderTexture.ReleaseTemporary(tempSplatMap);
        }

        public void Initialize()
        {
            // SurfaceData is created before this method is called...
            
            surfaceData.CreateBrushMaterial();
            surfaceData.CreatePaintedSurfaceMaterial();
            surfaceData.CreateDeformPaint();
            surfaceData.UpdateMainTextures();
            surfaceData.UpdateDeformTextures();
            surfaceData.UpdateSurfaceTextures();
        }

        public void Load()
        {
//            surfaceData.UpdateMainTextures();
//            surfaceData.UpdateDeformTextures();
//            surfaceData.UpdateSurfaceTextures();
        }

        public void UpdateMaterial()
        {
            MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
            meshRenderer.material = surfaceData.PaintedSurfaceMaterial;
        }

        public void CreateBrushMaterial()
        {
            surfaceData.CreateBrushMaterial();
        }

        public void CreatePaintedSurfaceMaterial()
        {
            surfaceData.CreatePaintedSurfaceMaterial();
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

        public void AddPaint()
        {
            surfaceData.AddPaint();
        }

        public void RemovePaint()
        {
            surfaceData.RemovePaint();
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