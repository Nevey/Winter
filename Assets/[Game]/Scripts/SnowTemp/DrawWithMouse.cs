using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class DrawWithMouse : MonoBehaviour
{
    public Camera camera;

    public Shader drawShader;

    [Range(1, 500)] public float brushSize;
    
    [Range(0, 1)] public float brushStrength;

    private RenderTexture splatMap;

    private Material snowMaterial, drawMaterial;
    
    private RaycastHit hit;
    
    // Start is called before the first frame update
    void Start()
    {
        drawMaterial = new Material(drawShader);
        drawMaterial.SetVector("_Color", Color.red);

        snowMaterial = GetComponent<MeshRenderer>().material;
        splatMap = new RenderTexture(1024, 1024, 0, RenderTextureFormat.ARGBFloat);
        snowMaterial.SetTexture("_Splat", splatMap);
    }

    private void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.Mouse0))
        {
            if (Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out hit))
            {
                drawMaterial.SetVector("_Coordinate", new Vector4(hit.textureCoord.x, hit.textureCoord.y, 0f, 0f));
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

    private void OnGUI()
    {
        GUI.DrawTexture(new Rect(0, 0, 256, 256), splatMap, ScaleMode.ScaleToFit, false, 1);
    }
}
