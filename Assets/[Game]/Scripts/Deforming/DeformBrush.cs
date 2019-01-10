using Game.Utilities;
using UnityEngine;

namespace Game.Deforming
{
    [ExecuteInEditMode]
    public class DeformBrush : MonoBehaviour
    {
        public const int MAX_PAINTS = 5;
        
        [SerializeField] private float brushSize;

        [SerializeField] private float brushOpacity;

        [SerializeField] private Texture defaultTexture;

        [SerializeField] private Texture defaultNormalMap;

        [SerializeField] private int defaultTextureTiling;

        [SerializeField] private Paint[] paints = new Paint[0];

        [SerializeField] private int selectedPaintIndex;

        [SerializeField] private Shader brushShader;

        [SerializeField] private Shader paintedSurfaceShader;
        
        private Material paintedSurfaceMaterial;

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
                RenderTextureFormat.ARGBFloat);
            
            Graphics.Blit(alphaMap, tempSplatMap);
            Graphics.Blit(tempSplatMap, alphaMap, drawMaterial);
            
            RenderTexture.ReleaseTemporary(tempSplatMap);
        }

        public void Draw(Vector2 textureCoord)
        {
            DrawOnAlphaMap(textureCoord, paints[selectedPaintIndex].BrushMaterial, paints[selectedPaintIndex].AlphaMap, 1);

            for (int i = 0; i < paints.Length; i++)
            {
                if (i == selectedPaintIndex)
                {
                    continue;
                }
                
                DrawOnAlphaMap(textureCoord, paints[i].BrushMaterial, paints[i].AlphaMap, 0);
            }
        }

        public void Erase(Vector2 textureCoord)
        {
            for (int i = 0; i < paints.Length; i++)
            {
                DrawOnAlphaMap(textureCoord, paints[i].BrushMaterial, paints[i].AlphaMap, 0);
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

            if (defaultNormalMap == null)
            {
                defaultNormalMap = paintedSurfaceMaterial.GetTexture("_MainNormal");
            }
            else
            {
                paintedSurfaceMaterial.SetTexture("_MainNormal", defaultNormalMap);
            }

            if (defaultTextureTiling < 1)
            {
                defaultTextureTiling = 1;
            }

            Vector2 tiling = new Vector2(defaultTextureTiling, defaultTextureTiling);
            
            paintedSurfaceMaterial.SetTextureScale("_MainTex", tiling);
            paintedSurfaceMaterial.SetTextureScale("_MainNormal", tiling);
        }

        public void AddPaint()
        {
            if (paintedSurfaceMaterial == null)
            {
                SetDefaultTextures();
            }
            
            if (paints.Length == MAX_PAINTS)
            {
                return;
            }
            
            if (brushShader == null)
            {
                throw Log.Exception("No brush shader was set! Not adding a new paint layer...");
            }
            
            Paint[] newPaintArray = new Paint[paints.Length + 1];

            for (int i = 0; i < newPaintArray.Length; i++)
            {
                if (i > paints.Length - 1)
                {
                    Paint paint = new Paint(brushShader);
                    paint.NormalMap = paintedSurfaceMaterial.GetTexture("_PaintNormal" + i);
                    
                    newPaintArray[i] = paint;
                    
                    continue;
                }
                
                newPaintArray[i] = paints[i];
            }

            paints = newPaintArray;
        }

        public void RemovePaint()
        {
            if (paints.Length == 0)
            {
                return;
            }
            
            Paint[] newPaintArray = new Paint[paints.Length - 1];

            for (int i = 0; i < newPaintArray.Length; i++)
            {
                newPaintArray[i] = paints[i];
            }

            paints = newPaintArray;
        }

        public void UpdatePaintSettings()
        {
            if (paintedSurfaceMaterial == null)
            {
                SetDefaultTextures();
            }
            
            for (int i = 0; i < MAX_PAINTS; i++)
            {
                if (i <= paints.Length - 1)
                {
                    Paint paint = paints[i];
                    
                    paintedSurfaceMaterial.SetTexture("_PaintTex" + i, paint.PaintTexture);
                    paintedSurfaceMaterial.SetTexture("_PaintNormal" + i, paint.NormalMap);
                    paintedSurfaceMaterial.SetTexture("_PaintAlpha" + i, paint.AlphaMap);
                    
                    Vector2 tiling = new Vector2(paint.Tiling, paint.Tiling);
                
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