using System;
using UnityEngine;

namespace Game.Surfaces
{
    [Serializable]
    public class SurfacePaint
    {
        [SerializeField] private Texture paintTexture;

        [SerializeField] private Texture normalMap;

        [SerializeField] private RenderTexture alphaMap;

        [SerializeField] private int tiling = 1;

        public Texture PaintTexture => paintTexture;

        public Texture NormalMap => normalMap;

        public RenderTexture AlphaMap => alphaMap;

        public int Tiling
        {
            get
            {
                if (tiling < 1)
                {
                    tiling = 1;
                }

                return tiling;
            }
        }

        public SurfacePaint(string surfaceName)
        {
            alphaMap = new RenderTexture(1024, 1024, 0, RenderTextureFormat.ARGB32)
            {
                name = $"surfaceAlphaMap_{surfaceName}"
            };
        }

        public void SetAlphaMap(Texture2D tex)
        {
            Graphics.Blit(tex, alphaMap);
        }
    }
}