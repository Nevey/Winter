using System;
using UnityEngine;

namespace Game.Deforming
{
    [Serializable]
    internal class DeformPaint
    {
        [SerializeField] private Texture deformTexture;

        [SerializeField] private Texture dispMap;

        [SerializeField] private RenderTexture alphaMap;

        [SerializeField] private Material brushMaterial;

        [SerializeField] private int tiling = 1;

        public Texture DeformTexture => deformTexture;

        public Texture DispMap
        {
            get => dispMap;
            set => dispMap = value;
        }

        public RenderTexture AlphaMap => alphaMap;

        public Material BrushMaterial => brushMaterial;

        public int Tiling => tiling;

        public DeformPaint(Shader brushShader)
        {
            alphaMap = new RenderTexture(1024, 1024, 0, RenderTextureFormat.ARGB32);
            
            brushMaterial = new Material(brushShader);
        }
    }
}