using UnityEngine;
using System.Collections;

namespace DS
{
    public static class SurfaceMeshBuilder
    {
        public static Vector3[] GetVertices(SurfaceMeshParams p)
        {
            Vector3[] verts = new Vector3[p.Size];

            int vertIndex = 0;
            for (int i = 0; i < p.Length; i++)
                for (int j = 0; j < p.Width; j++)
                    verts[vertIndex++] = new Vector3(j * p.Scale, 0, i * p.Scale);

            return verts;
        }

        public static Vector3[] GetVertices(SurfaceMeshParams p, Texture2D baseMap, float offsetMul, int heightmapResolution)
        {
            Vector3[] verts = new Vector3[p.Size];
            
            int vertIndex = 0;
            for (int i = 0; i < p.Length; i++)
            {
                for (int j = 0; j < p.Width; j++)
                {
                    Vector2 uv = new Vector2((float)(j * heightmapResolution) / baseMap.width, (float)(i * heightmapResolution) / baseMap.height);
                    float height = baseMap.GetPixelBilinear(uv.x, uv.y).r * offsetMul;
                    verts[vertIndex++] = new Vector3(j * p.Scale, height, i * p.Scale);
                }
            }
                
            return verts;
        }

        public static int[] GetIndicesQuad(SurfaceMeshParams p)
        {
            int[] quads = new int[(p.Width - 1) * (p.Length - 1) * 4];

            int qIndex = 0;
            for (int i = 0; i < (p.Width * p.Length) - p.Width - 1; i++)
            {
                if ((i + 1) % p.Width == 0)
                    continue;

                quads[qIndex++] = i + 1;
                quads[qIndex++] = i;
                quads[qIndex++] = i + p.Width;
                quads[qIndex++] = i + p.Width + 1;
            }

            return quads;
        }

        public static int[] GetTriangles(SurfaceMeshParams p)
        {
            int[] tris = new int[(p.Width - 1) * (p.Length - 1) * 6];

            int trisIndex = 0;
            for (int i = 0; i < (p.Width * p.Length) - p.Width - 1; i++)
            {
                if ((i + 1) % p.Width == 0)
                    continue;

                tris[trisIndex++] = i;
                tris[trisIndex++] = i + p.Width;
                tris[trisIndex++] = i + p.Width + 1;

                tris[trisIndex++] = i;
                tris[trisIndex++] = i + p.Width + 1;
                tris[trisIndex++] = i + 1;
            }

            return tris;
        }

        public static Vector4[] GetTangents(SurfaceMeshParams p)
        {
            Vector4[] tan = new Vector4[p.Size];

            for (int i = 0; i < tan.Length; i++)
                tan[i] = new Vector4(1, 0, 0, -1);

            return tan;
        }

        public static Vector2[] GetUV1(SurfaceMeshParams p)
        {
            Vector2[] uvs = new Vector2[p.Size];

            for (int i = 0; i < p.Length; i++)
                for (int j = 0; j < p.Width; j++)
                    uvs[j + p.Width * i] = new Vector2(j, i);

            return uvs;
        }

        public static Vector2[] GetUV2(SurfaceMeshParams p)
        {
            Vector2[] uvs = new Vector2[p.Size];

            for (int i = 0; i < p.Length; i++)
                for (int j = 0; j < p.Width; j++)
                    uvs[j + p.Width * i] = new Vector2((float)j / (p.Width - 1), (float)i / (p.Length - 1));

            return uvs;
        }

        public static Bounds GetBounds(SurfaceMeshParams p, float maxHeight)
        {
            Vector2 wl = new Vector2((p.Width - 1) * p.Scale, (p.Length - 1) * p.Scale);

            Vector3 center = new Vector3(wl.x * 0.5f, maxHeight * 0.5f, wl.y * 0.5f);
            Vector3 size = new Vector3(wl.x, maxHeight, wl.y);

            return new Bounds(center, size);
        }
    }

    public struct SurfaceMeshParams
    {
        /// <summary>
        /// Mesh width in units
        /// </summary>
        public int Width;
        /// <summary>
        /// Mesh length in units
        /// </summary>
        public int Length;
        /// <summary>
        /// Mesh vertices distance multiplier
        /// </summary>
        public float Scale { get; private set; }
        /// <summary>
        /// Total mesh vertices count. Returns <see cref="Width"/> * <see cref="Length"/>
        /// </summary>
        public int Size { get; private set; }

        public SurfaceMeshParams(int width, int length, float scale) : this()
        {
            Width = width + 1;
            Length = length + 1;
            Scale = scale;
            Size = Width * Length;
        }
    }

}



