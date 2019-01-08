using System.Runtime.CompilerServices;
using Game.Utilities;
using UnityEngine;

namespace Game.Deforming
{
    [ExecuteInEditMode]
    public class DeformBrush : MonoBehaviour
    {
        [SerializeField] private float brushSize;

        [SerializeField] private float brushOpacity;

        [SerializeField] private Paint[] paints;

        // TODO: Add Material layers, shaders will be set automatically there
        [SerializeField] private Shader drawShader;

        [SerializeField] private Shader paintedSurfaceShader;
        
        [SerializeField] private RenderTexture alphaMap;

        private Material paintedSurfaceMaterial;
        
        private Material drawMaterial;

        
//        private Material deformMaterial;

        public void Initialize()
        {
            if (paints != null)
            {
                for (int i = 0; i < paints.Length; i++)
                {
                    paints[i].CreateRenderTexture(drawShader);
                }
            }

            if (drawShader != null)
            {
                drawMaterial = new Material(drawShader);
            }
            
            
//            alphaMap = new RenderTexture(1024, 1024, 0, RenderTextureFormat.ARGBFloat);

            if (paintedSurfaceShader != null)
            {
                paintedSurfaceMaterial = new Material(paintedSurfaceShader);
                
                MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
                meshRenderer.material = paintedSurfaceMaterial;
                
                // Convert existing splat texture to the new render texture
                Texture currentTexture = paintedSurfaceMaterial.GetTexture("_AlphaMap");
                
                Graphics.Blit(currentTexture, alphaMap);
                
                paintedSurfaceMaterial.SetTexture("_AlphaMap", alphaMap);
            }

        }

        public void Draw(Vector2 textureCoord)
        {
//            for (int i = 0; i < paints.Length; i++)
//            {
//                
//            }
            
            DrawAlphaMap(textureCoord);
        }

        private void DrawAlphaMap(Vector2 textureCoord)
        {
            drawMaterial.SetVector("_Coordinate", new Vector4(textureCoord.x, textureCoord.y, 0f, 0f));
            drawMaterial.SetFloat("_Strength", brushOpacity);
            drawMaterial.SetFloat("_Size", brushSize);
            
            RenderTexture tempSplatMap = RenderTexture.GetTemporary(alphaMap.width, alphaMap.height, 0,
                RenderTextureFormat.ARGBFloat);
            
            Graphics.Blit(alphaMap, tempSplatMap);
            Graphics.Blit(tempSplatMap, alphaMap, drawMaterial);
            
            RenderTexture.ReleaseTemporary(tempSplatMap);
        }

        public void AddPaint()
        {
            Paint[] newPaintArray = new Paint[paints.Length + 1];

            for (int i = 0; i < newPaintArray.Length; i++)
            {
                if (i > paints.Length - 1)
                {
                    newPaintArray[i] = new Paint();
                    continue;
                }
                
                newPaintArray[i] = paints[i];
            }

            paints = newPaintArray;
        }

        public void RemovePaint()
        {
            Paint[] newPaintArray = new Paint[paints.Length - 1];

            for (int i = 0; i < newPaintArray.Length; i++)
            {
                newPaintArray[i] = paints[i];
            }

            paints = newPaintArray;
        }

        private void OnGUI()
        {
            GUI.DrawTexture(new Rect(0, 0, 256, 256), alphaMap, ScaleMode.ScaleToFit, false, 1);
        }
    }
}