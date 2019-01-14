using Game.Utilities;

// TODO: Remove Editor dependencies for builds
using UnityEditor;
using UnityEngine;

namespace Game.Deforming
{
    public class SurfaceData : ScriptableObject
    {
        public const int MAX_PAINTS = 5;
        
        // Is reference to Asset in project
        [SerializeField] private Shader brushShader;
        [SerializeField] private Shader paintedSurfaceShader;
        [SerializeField] private Texture mainTexture;
        [SerializeField] private Texture mainNormal;
        [SerializeField] private int mainTextureTiling;

        // Is added as Asset to instance of this ScriptableObject
        [SerializeField] private Material brushMaterial;
        [SerializeField] private Material paintedSurfaceMaterial;
        [SerializeField] private SurfacePaint[] surfacePaints = new SurfacePaint[0];
        [SerializeField] private DeformPaint deformPaint;

        public Material PaintedSurfaceMaterial => paintedSurfaceMaterial;

        public SurfacePaint[] SurfacePaints => surfacePaints;

        public DeformPaint DeformPaint => deformPaint;

        public void CreatePaintedSurfaceMaterial()
        {
            if (paintedSurfaceShader == null)
            {
                return;
            }
            
            paintedSurfaceMaterial = new Material(paintedSurfaceShader);
            
            AssetDatabase.AddObjectToAsset(paintedSurfaceMaterial, this);
        }

        public void CreateDeformPaint()
        {
            deformPaint = new DeformPaint(brushShader);
                
            AssetDatabase.AddObjectToAsset(deformPaint.AlphaMap, this);
        }
        
        public void UpdateMainTextures()
        {
            if (paintedSurfaceMaterial == null)
            {
                return;
            }
            
            paintedSurfaceMaterial.SetTexture("_MainTex", mainTexture);
            paintedSurfaceMaterial.SetTexture("_MainNormal", mainNormal);
            
            if (mainTextureTiling < 1)
            {
                mainTextureTiling = 1;
            }

            Vector2 tiling = new Vector2(mainTextureTiling, mainTextureTiling);
            
            paintedSurfaceMaterial.SetTextureScale("_MainTex", tiling);
            paintedSurfaceMaterial.SetTextureScale("_MainNormal", tiling);
        }
        
        public void UpdateDeformTextures()
        {
            if (paintedSurfaceMaterial == null)
            {
                return;
            }
            
            paintedSurfaceMaterial.SetTexture("_DeformTex", deformPaint.DeformTexture);
            paintedSurfaceMaterial.SetTexture("_DeformDispTex", deformPaint.DispMap);
            paintedSurfaceMaterial.SetTexture("_DeformAlpha", deformPaint.AlphaMap);
            
            Vector2 tiling = new Vector2(deformPaint.Tiling, deformPaint.Tiling);
            
            paintedSurfaceMaterial.SetTextureScale("_DeformTex", tiling);
            paintedSurfaceMaterial.SetTextureScale("_DeformDispTex", tiling);
        }
        
        public void UpdateSurfaceTextures()
        {
            if (paintedSurfaceMaterial == null)
            {
                return;
            }
            
            for (int i = 0; i < MAX_PAINTS; i++)
            {
                if (i <= surfacePaints.Length - 1)
                {
                    // Setup texture slots
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
                    // Reset unused texture slots
                    paintedSurfaceMaterial.SetTexture("_PaintTex" + i, null);
                    paintedSurfaceMaterial.SetTexture("_PaintNormal" + i, null);
                    paintedSurfaceMaterial.SetTexture("_PaintAlpha" + i, null);
                    
                    Vector2 tiling = new Vector2(1, 1);
                
                    paintedSurfaceMaterial.SetTextureScale("_PaintTex" + i, tiling);
                    paintedSurfaceMaterial.SetTextureScale("_PaintNormal" + i, tiling);
                }
            }
        }
        
        public void AddPaint()
        {
            if (surfacePaints.Length == MAX_PAINTS)
            {
                return;
            }
            
            SurfacePaint[] newSurfacePaintArray = new SurfacePaint[surfacePaints.Length + 1];

            for (int i = 0; i < newSurfacePaintArray.Length; i++)
            {
                if (i > surfacePaints.Length - 1)
                {
                    SurfacePaint surfacePaint = new SurfacePaint(brushShader);
                    
                    // Add objects to the Asset
                    AssetDatabase.AddObjectToAsset(surfacePaint.AlphaMap, this);

                    newSurfacePaintArray[i] = surfacePaint;
                    
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
            
            // Remove objects the Asset
            SurfacePaint surfacePaint = surfacePaints[surfacePaints.Length - 1];
            
            AssetDatabase.RemoveObjectFromAsset(surfacePaint.AlphaMap);
            
            SurfacePaint[] newSurfacePaintArray = new SurfacePaint[surfacePaints.Length - 1];

            for (int i = 0; i < newSurfacePaintArray.Length; i++)
            {
                newSurfacePaintArray[i] = surfacePaints[i];
            }

            surfacePaints = newSurfacePaintArray;
        }
    }
}