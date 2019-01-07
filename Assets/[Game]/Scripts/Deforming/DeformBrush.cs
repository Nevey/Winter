using Game.Utilities;
using UnityEngine;

namespace Game.Building
{
    [ExecuteInEditMode]
    public class DeformBrush : MonoBehaviour
    {
        [SerializeField, Range(0, 500)] private float brushSize;

        [SerializeField, Range(0, 1)] private float brushStrength;

        [SerializeField] private Shader drawShader;
        
        private Material drawMaterial;

        private Material deformMaterial;
        
        private RenderTexture alphaMap;
        
        private void Reset()
        {
            drawMaterial = new Material(drawShader);

            MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
            
            deformMaterial = meshRenderer.material;
            
            // Convert existing splat texture to the new render texture
            alphaMap = new RenderTexture(1024, 1024, 0, RenderTextureFormat.ARGBFloat);

            Texture currentTexture = deformMaterial.GetTexture("_AlphaMap");
            
            Graphics.Blit(currentTexture, alphaMap);
            
            deformMaterial.SetTexture("_AlphaMap", alphaMap);
        }

        public void DrawStuff(Vector2 textureCoord)
        {
            drawMaterial.SetVector("_Coordinate", new Vector4(textureCoord.x, textureCoord.y, 0f, 0f));
            drawMaterial.SetFloat("_Strength", brushStrength);
            drawMaterial.SetFloat("_Size", brushSize);
            
            RenderTexture tempSplatMap = RenderTexture.GetTemporary(alphaMap.width, alphaMap.height, 0,
                RenderTextureFormat.ARGBFloat);
            
            Graphics.Blit(alphaMap, tempSplatMap);
            Graphics.Blit(tempSplatMap, alphaMap, drawMaterial);
            
            RenderTexture.ReleaseTemporary(tempSplatMap);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                Debug.Log("shalala");
            }
            
            return;

            Ray ray = Camera.current.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f));

            if (!Physics.Raycast(ray, out RaycastHit hit, 1000f))
            {
                return;
            }

            if (Camera.current == null)
            {
                return;
            }
            
            Log.Write(hit.transform.name);
            
//            drawMaterial.SetVector("_Coordinate", new Vector4(hit.textureCoord.x, hit.textureCoord.y, 0f, 0f));
//            drawMaterial.SetFloat("_Strength", brushStrength);
//            drawMaterial.SetFloat("_Size", brushSize);
//                
//            RenderTexture tempSplatMap = RenderTexture.GetTemporary(alphaMap.width, alphaMap.height, 0,
//                RenderTextureFormat.ARGBFloat);
//                
//            Graphics.Blit(alphaMap, tempSplatMap);
//            Graphics.Blit(tempSplatMap, alphaMap, drawMaterial);
//                
//            RenderTexture.ReleaseTemporary(tempSplatMap);
        }
    }
}