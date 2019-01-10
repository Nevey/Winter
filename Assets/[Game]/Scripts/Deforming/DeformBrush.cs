using Game.Utilities;
using UnityEngine;

namespace Game.Deforming
{
    public class DeformBrush : MonoBehaviour
    {
        public const int MAX_PAINTS = 5;
        
        [SerializeField] private float brushSize;

        [SerializeField] private float brushOpacity;

        [SerializeField] private Texture defaultTexture;

        [SerializeField] private Texture defaultNormalMap;

        [SerializeField] private int defaultTextureTiling = 1;
        
        [SerializeField] private DeformPaint deformPaint;

        [SerializeField] private int paintType;
        
        [SerializeField] private SurfacePaint[] surfacePaints = new SurfacePaint[0];

        [SerializeField] private int selectedPaintIndex;

        [SerializeField] private Shader brushShader;

        [SerializeField] private Shader paintedSurfaceShader;
        
        private Material paintedSurfaceMaterial;

        private void DrawSurface(Vector2 textureCoord)
        {
            if (selectedPaintIndex == -1)
            {
                return;
            }
            
            if (selectedPaintIndex > surfacePaints.Length - 1)
            {
                selectedPaintIndex = surfacePaints.Length - 1;
            }
            
            DrawOnAlphaMap(textureCoord, surfacePaints[selectedPaintIndex].BrushMaterial, surfacePaints[selectedPaintIndex].AlphaMap, 1);

            for (int i = 0; i < surfacePaints.Length; i++)
            {
                if (i == selectedPaintIndex)
                {
                    continue;
                }
                
                DrawOnAlphaMap(textureCoord, surfacePaints[i].BrushMaterial, surfacePaints[i].AlphaMap, 0);
            }
        }

        private void DrawDeform(Vector2 textureCoord)
        {
            DrawOnAlphaMap(textureCoord, deformPaint.BrushMaterial, deformPaint.AlphaMap, 1);
        }

        private void EraseSurface(Vector2 textureCoord)
        {
            for (int i = 0; i < surfacePaints.Length; i++)
            {
                DrawOnAlphaMap(textureCoord, surfacePaints[i].BrushMaterial, surfacePaints[i].AlphaMap, 0);
            }
        }

        private void EraseDeform(Vector2 textureCoord)
        {
            DrawOnAlphaMap(textureCoord, deformPaint.BrushMaterial, deformPaint.AlphaMap, 0);
        }
        
        private void DrawOnAlphaMap(Vector2 textureCoord, Material drawMaterial, RenderTexture alphaMap, int appends = 1)
        {
            if (drawMaterial == null || alphaMap == null)
            {
                return;
            }
            
            drawMaterial.SetFloat("_Appends", appends);
            drawMaterial.SetVector("_Coordinate", new Vector4(textureCoord.x, textureCoord.y, 0f, 0f));
            drawMaterial.SetFloat("_Strength", brushOpacity);
            drawMaterial.SetFloat("_Size", brushSize);
            
            RenderTexture tempSplatMap = RenderTexture.GetTemporary(alphaMap.width, alphaMap.height, 0,
                RenderTextureFormat.ARGB32);
            
            Graphics.Blit(alphaMap, tempSplatMap);
            Graphics.Blit(tempSplatMap, alphaMap, drawMaterial);
            
            RenderTexture.ReleaseTemporary(tempSplatMap);
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
                EraseSurface(textureCoord);
            }
            else
            {
                EraseDeform(textureCoord);
            }
        }

        public void SetDefaultTextures()
        {
            if (defaultTexture == null)
            {
                return;
            }
            
            // Create new material if it wasn't created yet...
            if (paintedSurfaceShader != null && paintedSurfaceMaterial == null)
            {
                paintedSurfaceMaterial = new Material(paintedSurfaceShader);
                MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
                meshRenderer.material = paintedSurfaceMaterial;
            }
                    
            paintedSurfaceMaterial.SetTexture("_MainTex", defaultTexture);

            paintedSurfaceMaterial.SetTexture("_MainNormal", defaultNormalMap);
            
            if (defaultTextureTiling < 1)
            {
                defaultTextureTiling = 1;
            }

            Vector2 tiling = new Vector2(defaultTextureTiling, defaultTextureTiling);
            
            paintedSurfaceMaterial.SetTextureScale("_MainTex", tiling);
            paintedSurfaceMaterial.SetTextureScale("_MainNormal", tiling);
        }

        public void CheckDeformSetup()
        {
            if (deformPaint == null
                || deformPaint.AlphaMap == null
                || deformPaint.BrushMaterial == null)
            {
                deformPaint = new DeformPaint(brushShader);
            }
        }

        public void SetDeformTextures()
        {
            if (paintedSurfaceMaterial == null)
            {
                SetDefaultTextures();
            }
            
            CheckDeformSetup();
            
            paintedSurfaceMaterial.SetTexture("_DeformTex", deformPaint.DeformTexture);
            paintedSurfaceMaterial.SetTexture("_DeformDispTex", deformPaint.DispMap);
            paintedSurfaceMaterial.SetTexture("_DeformAlpha", deformPaint.AlphaMap);
            
            Vector2 tiling = new Vector2(deformPaint.Tiling, deformPaint.Tiling);
            
            paintedSurfaceMaterial.SetTextureScale("_DeformTex", tiling);
            paintedSurfaceMaterial.SetTextureScale("_DeformDispTex", tiling);
        }

        public void AddPaint()
        {
            if (paintedSurfaceMaterial == null)
            {
                SetDefaultTextures();
            }
            
            if (surfacePaints.Length == MAX_PAINTS)
            {
                return;
            }
            
            if (brushShader == null)
            {
                throw Log.Exception("No brush shader was set! Not adding a new paint layer...");
            }
            
            SurfacePaint[] newSurfacePaintArray = new SurfacePaint[surfacePaints.Length + 1];

            for (int i = 0; i < newSurfacePaintArray.Length; i++)
            {
                if (i > surfacePaints.Length - 1)
                {
                    newSurfacePaintArray[i] = new SurfacePaint(brushShader);   
                    continue;
                }
                
                newSurfacePaintArray[i] = surfacePaints[i];
            }

            surfacePaints = newSurfacePaintArray;
        }

        public void RemovePaint()
        {
            if (surfacePaints.Length == 0)
            {
                return;
            }
            
            SurfacePaint[] newSurfacePaintArray = new SurfacePaint[surfacePaints.Length - 1];

            for (int i = 0; i < newSurfacePaintArray.Length; i++)
            {
                newSurfacePaintArray[i] = surfacePaints[i];
            }

            surfacePaints = newSurfacePaintArray;
        }

        public void UpdatePaintSettings()
        {
            if (paintedSurfaceMaterial == null)
            {
                SetDefaultTextures();
            }
            
            for (int i = 0; i < MAX_PAINTS; i++)
            {
                if (i <= surfacePaints.Length - 1)
                {
                    SurfacePaint surfacePaint = surfacePaints[i];
                    
                    paintedSurfaceMaterial.SetTexture("_PaintTex" + i, surfacePaint.PaintTexture);
                    paintedSurfaceMaterial.SetTexture("_PaintNormal" + i, surfacePaint.NormalMap);
                    paintedSurfaceMaterial.SetTexture("_PaintAlpha" + i, surfacePaint.AlphaMap);
                    
                    Vector2 tiling = new Vector2(surfacePaint.Tiling, surfacePaint.Tiling);
                
                    paintedSurfaceMaterial.SetTextureScale("_PaintTex" + i, tiling);
                    paintedSurfaceMaterial.SetTextureScale("_PaintNormal" + i, tiling);
                }
                else
                {
                    paintedSurfaceMaterial.SetTexture("_PaintTex" + i, null);
                    paintedSurfaceMaterial.SetTexture("_PaintNormal" + i, null);
                    paintedSurfaceMaterial.SetTexture("_PaintAlpha" + i, null);
                    
                    Vector2 tiling = new Vector2(1, 1);
                
                    paintedSurfaceMaterial.SetTextureScale("_PaintTex" + i, tiling);
                    paintedSurfaceMaterial.SetTextureScale("_PaintNormal" + i, tiling);
                }
            }
        }
    }
}