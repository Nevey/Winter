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

        [SerializeField] private Texture defaultTexture;

        [SerializeField] private Texture defaultNormalMap;

        [SerializeField] private int defaultTextureTiling = 1;

        [SerializeField] private int paintType;

        [SerializeField] private int selectedPaintIndex = -1;

        [SerializeField] private Shader brushShader;

        [SerializeField] private Shader paintedSurfaceShader;

        [SerializeField] private SurfaceData surfaceData;
        
        private Material paintedSurfaceMaterial;

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

            Material paintedBrushMaterial = surfaceData.SurfacePaints[selectedPaintIndex].BrushMaterial;
            RenderTexture paintedAlphaMap = surfaceData.SurfacePaints[selectedPaintIndex].AlphaMap;
            
            DrawOnAlphaMap(textureCoord, ref paintedBrushMaterial, ref paintedAlphaMap, 1);
            
            surfaceData.SurfacePaints[selectedPaintIndex].BrushMaterial = paintedBrushMaterial;
            surfaceData.SurfacePaints[selectedPaintIndex].AlphaMap = paintedAlphaMap;

            for (int i = 0; i < surfaceData.SurfacePaints.Length; i++)
            {
                if (i == selectedPaintIndex)
                {
                    continue;
                }
                
                Material erasedBrushMaterial = surfaceData.SurfacePaints[i].BrushMaterial;
                RenderTexture erasedAlphaMap = surfaceData.SurfacePaints[i].AlphaMap;
                
                DrawOnAlphaMap(textureCoord, ref erasedBrushMaterial, ref erasedAlphaMap, 0);
                
                surfaceData.SurfacePaints[i].BrushMaterial = erasedBrushMaterial;
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
                Material erasedBrushMaterial = surfaceData.SurfacePaints[i].BrushMaterial;
                RenderTexture erasedAlphaMap = surfaceData.SurfacePaints[i].AlphaMap;
                
                DrawOnAlphaMap(textureCoord, ref erasedBrushMaterial, ref erasedAlphaMap, 0);
                
                surfaceData.SurfacePaints[i].BrushMaterial = erasedBrushMaterial;
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
        
        private void DrawOnAlphaMap(Vector2 textureCoord, ref Material drawMaterial, ref RenderTexture alphaMap, int appends = 1)
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

        public void Initialize()
        {
            SetDefaultTextures();
            SetDeformTextures();
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

        public void SetDeformTextures()
        {
            if (paintedSurfaceMaterial == null)
            {
                SetDefaultTextures();
            }
            
            CheckDeformSetup();
            
            paintedSurfaceMaterial.SetTexture("_DeformTex", surfaceData.DeformPaint.DeformTexture);
            paintedSurfaceMaterial.SetTexture("_DeformDispTex", surfaceData.DeformPaint.DispMap);
            paintedSurfaceMaterial.SetTexture("_DeformAlpha", surfaceData.DeformPaint.AlphaMap);
            
            Vector2 tiling = new Vector2(surfaceData.DeformPaint.Tiling, surfaceData.DeformPaint.Tiling);
            
            paintedSurfaceMaterial.SetTextureScale("_DeformTex", tiling);
            paintedSurfaceMaterial.SetTextureScale("_DeformDispTex", tiling);
        }

        public void CheckDeformSetup()
        {
            if (surfaceData.DeformPaint == null
                || surfaceData.DeformPaint.AlphaMap == null
                || surfaceData.DeformPaint.BrushMaterial == null)
            {
                surfaceData.DeformPaint = new DeformPaint(brushShader);
                AssetDatabase.AddObjectToAsset(surfaceData.DeformPaint.AlphaMap, surfaceData);
                AssetDatabase.AddObjectToAsset(surfaceData.DeformPaint.BrushMaterial, surfaceData);
            }
        }

        public void AddPaint()
        {
            if (paintedSurfaceMaterial == null)
            {
                SetDefaultTextures();
            }
            
            if (surfaceData.SurfacePaints.Length == MAX_PAINTS)
            {
                return;
            }
            
            if (brushShader == null)
            {
                throw Log.Exception("No brush shader was set! Not adding a new paint layer...");
            }
            
            SurfacePaint[] newSurfacePaintArray = new SurfacePaint[surfaceData.SurfacePaints.Length + 1];

            for (int i = 0; i < newSurfacePaintArray.Length; i++)
            {
                if (i > surfaceData.SurfacePaints.Length - 1)
                {
                    SurfacePaint surfacePaint = new SurfacePaint(brushShader);
                    
                    AssetDatabase.AddObjectToAsset(surfacePaint.AlphaMap, surfaceData);
                    AssetDatabase.AddObjectToAsset(surfacePaint.BrushMaterial, surfaceData);

                    newSurfacePaintArray[i] = surfacePaint;
                    
                    continue;
                }
                
                newSurfacePaintArray[i] = surfaceData.SurfacePaints[i];
            }

            surfaceData.SurfacePaints = newSurfacePaintArray;
        }

        public void RemovePaint()
        {
            if (surfaceData.SurfacePaints.Length == 0)
            {
                return;
            }
            
            // Remove objects from the SurfaceData asset
            SurfacePaint surfacePaint = surfaceData.SurfacePaints[surfaceData.SurfacePaints.Length - 1];
            AssetDatabase.RemoveObjectFromAsset(surfacePaint.BrushMaterial);
            AssetDatabase.RemoveObjectFromAsset(surfacePaint.AlphaMap);
            
            SurfacePaint[] newSurfacePaintArray = new SurfacePaint[surfaceData.SurfacePaints.Length - 1];

            for (int i = 0; i < newSurfacePaintArray.Length; i++)
            {
                newSurfacePaintArray[i] = surfaceData.SurfacePaints[i];
            }

            surfaceData.SurfacePaints = newSurfacePaintArray;
        }

        public void UpdatePaintSettings()
        {
            if (paintedSurfaceMaterial == null)
            {
                SetDefaultTextures();
            }
            
            for (int i = 0; i < MAX_PAINTS; i++)
            {
                if (i <= surfaceData.SurfacePaints.Length - 1)
                {
                    SurfacePaint surfacePaint = surfaceData.SurfacePaints[i];
                    
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