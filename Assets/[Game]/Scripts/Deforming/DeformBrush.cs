using Game.Utilities;
using UnityEngine;

namespace Game.Deforming
{
    public class DeformBrush : MonoBehaviour
    {
        [SerializeField] private float brushSize;

        [SerializeField] private float brushOpacity;

        [SerializeField] private Paint[] paints;

        // TODO: Add Material layers, shaders will be set automatically there
        [SerializeField] private Shader drawShader;
        
        [SerializeField] private RenderTexture alphaMap;
        
        private Material drawMaterial;

        
//        private Material deformMaterial;
        

        private void OnEnable()
        {
            
            
//            alphaMap = new RenderTexture(1024, 1024, 0, RenderTextureFormat.ARGBFloat);
        }

        public void Initialize()
        {
            for (int i = 0; i < paints.Length; i++)
            {
                paints[i].CreateRenderTexture(drawShader);
            }
            
            drawMaterial = new Material(drawShader);
            drawMaterial.SetTexture("_MainTex", alphaMap);
            
            
            
            
            

            MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
            
//            deformMaterial = meshRenderer.sharedMaterial;
            
            // Convert existing splat texture to the new render texture
            

//            Texture currentTexture = deformMaterial.GetTexture("_AlphaMap");
//            
//            Graphics.Blit(currentTexture, alphaMap);
//            
//            deformMaterial.SetTexture("_AlphaMap", alphaMap);
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
                RenderTextureFormat.ARGB32);
            
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
    }
}