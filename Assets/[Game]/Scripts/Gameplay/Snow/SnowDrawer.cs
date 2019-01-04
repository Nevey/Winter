using Game.Utilities;
using UnityEngine;

namespace Game.Gameplay.Snow
{
    public class SnowDrawer : MonoBehaviour
    {
        [SerializeField] private LayerMask layerMask;
        
        [SerializeField] private Shader drawShader;

        private RenderTexture splatMap;
        
        private Material snowMaterial;
        
        private Material drawMaterial;

        private void Awake()
        {
            SnowService.Instance.RegisterSnowDrawer(this);
            
            drawMaterial = new Material(drawShader);

            splatMap = new RenderTexture(1024, 1024, 0, RenderTextureFormat.ARGBFloat);

            MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
            Terrain terrain = GetComponent<Terrain>();

            if (terrain != null)
            {
                snowMaterial = terrain.materialTemplate;
            }
            else if (meshRenderer != null)
            {
                snowMaterial = meshRenderer.material;
            }
            else
            {
                throw Log.Exception("No proper material found to draw!");
            }
            
            snowMaterial.SetTexture("_Splat", splatMap);
        }

        private void OnDestroy()
        {
            SnowService.Instance.UnregisterSnowDrawer(this);
        }

        public void DrawStamp(Transform transform, float stampSize, float stampStrength)
        {
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 1f, layerMask))
            {
                drawMaterial.SetVector("_Coordinate", new Vector4(hit.textureCoord.x, hit.textureCoord.y, 0f, 0f));
                drawMaterial.SetFloat("_Strength", stampStrength);
                drawMaterial.SetFloat("_Size", stampSize);
                
                RenderTexture tempSplatMap = RenderTexture.GetTemporary(splatMap.width, splatMap.height, 0,
                    RenderTextureFormat.ARGBFloat);
                
                Graphics.Blit(splatMap, tempSplatMap);
                Graphics.Blit(tempSplatMap, splatMap, drawMaterial);
                
                RenderTexture.ReleaseTemporary(tempSplatMap);
            }
        }
    }
}