using System;
using UnityEngine;

namespace Game.Deforming
{
    [Serializable]
    internal class Paint
    {
        [SerializeField] private Texture paintTexture;

        [SerializeField] private Texture normalMap;

        [SerializeField] private RenderTexture alphaMap;

        [SerializeField] private Material brushMaterial;

        [SerializeField] private int tiling = 1;

        public Texture PaintTexture => paintTexture;

        public Texture NormalMap
        {
            get => normalMap;
            set => normalMap = value;
        }

        public RenderTexture AlphaMap => alphaMap;

        public Material BrushMaterial => brushMaterial;

        public int Tiling => tiling;

        public Paint(Shader brushShader)
        {
            alphaMap = new RenderTexture(1024, 1024, 0, RenderTextureFormat.ARGB32);
            
            brushMaterial = new Material(brushShader);
        }

        public void SetPaintTexture()
        {
            
        }

        public void SetNormalMap()
        {
            
        }
    }
}