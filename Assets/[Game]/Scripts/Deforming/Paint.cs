using System;
using UnityEngine;

namespace Game.Deforming
{
    [Serializable]
    public class Paint
    {
        [SerializeField] private Texture paintTexture;

        [SerializeField] private Texture normalMap;

        [SerializeField] private RenderTexture alphaMap;

        [SerializeField] private Material brushMaterial;

        [SerializeField] private int tiling = 1;

        public Texture PaintTexture => paintTexture;

        public Texture NormalMap => normalMap;

        public RenderTexture AlphaMap => alphaMap;

        public Material BrushMaterial => brushMaterial;

        public int Tiling => tiling;

        public Paint(Shader brushShader)
        {
            brushMaterial = new Material(brushShader);
        }
    }
}