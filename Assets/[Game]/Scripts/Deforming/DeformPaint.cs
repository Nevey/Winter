using System;
using UnityEngine;

namespace Game.Deforming
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

        public Texture DispMap
        {
            get => dispMap;
            set => dispMap = value;
        }

        public RenderTexture AlphaMap
        {
            get => alphaMap;
            set => alphaMap = value;
        }

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

        public DeformPaint()
        {
            alphaMap = new RenderTexture(1024, 1024, 0, RenderTextureFormat.ARGB32);
            alphaMap.name = "deformAlphaMap";
        }
    }
}