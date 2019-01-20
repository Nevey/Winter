using System;
using UnityEngine;

namespace Game.Surfaces
{
    [Serializable]
    public class DeformPaint
    {
        // Is reference to Asset in project
        [SerializeField] private Texture deformTexture;
        [SerializeField] private Texture dispMap;

        // Is added as Asset to instance of this ScriptableObject
        [SerializeField] private RenderTexture alphaMap;

        [SerializeField] private int tiling = 1;

        public Texture DeformTexture => deformTexture;

        public Texture DispMap => dispMap;

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

        public DeformPaint(string surfaceName)
        {
            alphaMap = new RenderTexture(1024, 1024, 0, RenderTextureFormat.ARGB32)
            {
                name = $"deformAlphaMap_{surfaceName}"
            };
        }
        
        public void SetAlphaMap(Texture2D tex)
        {
            Graphics.Blit(tex, alphaMap);
        }
    }
}