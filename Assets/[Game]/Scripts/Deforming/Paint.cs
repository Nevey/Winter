using System;
using UnityEngine;

namespace Game.Deforming
{
    [Serializable]
    public class Paint
    {
        // Texture
        // Normal map
        // Auto-create "paint map" (like heightmap but only for texture painting)

        [SerializeField] private Texture mainTexture;

        [SerializeField] private Texture normalMap;

        [SerializeField] private RenderTexture paintMap;

        [SerializeField] private Material paintMaterial; 

        public void CreateRenderTexture(Shader drawShader)
        {
            paintMap = new RenderTexture(512, 512, 0, RenderTextureFormat.Depth);
            
            paintMaterial = new Material(drawShader); 
        }
    }
}