using UnityEngine;

public class WheelTracks : MonoBehaviour
{
    public Shader drawShader;
    public GameObject terrain;
    public Transform[] wheels;
    [Range(1, 500)] public float brushSize;
    [Range(0, 1)] public float brushStrength;
    
    private Material snowMaterial, drawMaterial;
    private RaycastHit groundHit;
    private int layerMask;
    private RenderTexture splatMap;
    
    // Start is called before the first frame update
    void Start()
    {
        layerMask = LayerMask.GetMask("Ground");
        drawMaterial = new Material(drawShader);

        snowMaterial = terrain.GetComponent<MeshRenderer>().material;
        splatMap = new RenderTexture(1024, 1024, 0, RenderTextureFormat.ARGBFloat);
        snowMaterial.SetTexture("_Splat", splatMap);
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < wheels.Length; i++)
        {
            if (Physics.Raycast(wheels[i].position, Vector3.down, out groundHit, 1f, layerMask))
            {
                drawMaterial.SetVector("_Coordinate", new Vector4(groundHit.textureCoord.x, groundHit.textureCoord.y, 0f, 0f));
                drawMaterial.SetFloat("_Strength", brushStrength);
                drawMaterial.SetFloat("_Size", brushSize);
                
                RenderTexture temp = RenderTexture.GetTemporary(splatMap.width, splatMap.height, 0,
                    RenderTextureFormat.ARGBFloat);
                
                Graphics.Blit(splatMap, temp);
                Graphics.Blit(temp, splatMap, drawMaterial);
                
                RenderTexture.ReleaseTemporary(temp);
            }
        }
    }
}
