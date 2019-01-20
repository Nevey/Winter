using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.Surfaces
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
        [SerializeField] private List<SurfacePaint> surfacePaints = new List<SurfacePaint>();
        [SerializeField] private DeformPaint deformPaint;
        [SerializeField] private Texture2D[] alphaMapDataHolders;
        [SerializeField] private Texture2D deformMapDataHolder;

        public Material BrushMaterial => brushMaterial;

        public Material PaintedSurfaceMaterial => paintedSurfaceMaterial;

        public SurfacePaint[] SurfacePaints => surfacePaints.ToArray();

        public DeformPaint DeformPaint => deformPaint;

#if UNITY_EDITOR
        public void CreateBrushMaterial()
        {
            if (brushShader == null)
            {
                return;
            }
            
            brushMaterial = new Material(brushShader);
            
            AssetDatabase.AddObjectToAsset(brushMaterial, this);
        }

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
            deformPaint = new DeformPaint(name);
                
            AssetDatabase.AddObjectToAsset(deformPaint.AlphaMap, this);
        }
        
        public void CreateSurfacePaints()
        {
            for (int i = 0; i < MAX_PAINTS; i++)
            {
                SurfacePaint surfacePaint = new SurfacePaint(name);
                surfacePaints.Add(surfacePaint);
                    
                // Add objects to the Asset
                AssetDatabase.AddObjectToAsset(surfacePaint.AlphaMap, this);
            }
        }

        public void CreateAlphaMapDataHolders()
        {
            alphaMapDataHolders = new Texture2D[MAX_PAINTS];

            for (int i = 0; i < MAX_PAINTS; i++)
            {
                RenderTexture alphaMap = surfacePaints[i].AlphaMap;
                
                Texture2D surfaceTex = new Texture2D(alphaMap.width, alphaMap.height, TextureFormat.ARGB32, false);
                RenderTexture.active = alphaMap;
                surfaceTex.ReadPixels(new Rect(0, 0, alphaMap.width, alphaMap.height), 0, 0);
                surfaceTex.Apply();
                alphaMapDataHolders[i] = surfaceTex;
                
                AssetDatabase.AddObjectToAsset(surfaceTex, this);
            }

            deformMapDataHolder = new Texture2D(deformPaint.AlphaMap.width, deformPaint.AlphaMap.height,
                TextureFormat.ARGB32, false);
            
            RenderTexture.active = deformPaint.AlphaMap;
            deformMapDataHolder.ReadPixels(new Rect(0, 0, deformPaint.AlphaMap.width, deformPaint.AlphaMap.height), 0, 0);
            deformMapDataHolder.Apply();
            
            AssetDatabase.AddObjectToAsset(deformMapDataHolder, this);
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
        
        public void UpdateSurfaceTextures(bool init = false)
        {
            if (paintedSurfaceMaterial == null)
            {
                return;
            }
            
            for (int i = 0; i < MAX_PAINTS; i++)
            {
                SurfacePaint surfacePaint = surfacePaints[i];
                
                paintedSurfaceMaterial.SetTexture("_PaintTex" + i, surfacePaint.PaintTexture);
                paintedSurfaceMaterial.SetTexture("_PaintNormal" + i, surfacePaint.NormalMap);

                if (init)
                {
                    paintedSurfaceMaterial.SetTexture("_PaintAlpha" + i, surfacePaint.AlphaMap);
                }
                
                Vector2 tiling = new Vector2(surfacePaint.Tiling, surfacePaint.Tiling);
            
                paintedSurfaceMaterial.SetTextureScale("_PaintTex" + i, tiling);
                paintedSurfaceMaterial.SetTextureScale("_PaintNormal" + i, tiling);
            }
        }

        public void Save()
        {
            for (int i = 0; i < surfacePaints.Count; i++)
            {
                RenderTexture alphaMap = surfacePaints[i].AlphaMap;
                Texture2D tex = alphaMapDataHolders[i];
                RenderTexture.active = alphaMap;
                tex.ReadPixels(new Rect(0, 0, alphaMap.width, alphaMap.height), 0, 0);
                tex.Apply();
            }

            RenderTexture.active = deformPaint.AlphaMap;
            deformMapDataHolder.ReadPixels(new Rect(0, 0, deformPaint.AlphaMap.width, deformPaint.AlphaMap.height), 0, 0);
            deformMapDataHolder.Apply();
            
            AssetDatabase.SaveAssets();
            
            Load();
        }
#endif
        
        public void Load()
        {
            for (int i = 0; i < surfacePaints.Count; i++)
            {
                if (i > alphaMapDataHolders.Length - 1)
                {
                    break;
                }
                
                if (alphaMapDataHolders[i] == null)
                {
                    continue;
                }
                
                surfacePaints[i].SetAlphaMap(alphaMapDataHolders[i]);
            }

            if (deformMapDataHolder == null)
            {
                return;
            }
            
            deformPaint.SetAlphaMap(deformMapDataHolder);
        }
    }
}