using System;
using System.Text;
using UnityEngine;

namespace BlueGraphSamples
{
    /// <summary>
    /// Simple heightmap representation
    /// </summary>
    public class Heightmap
    {
        /// <summary>
        /// Number of samples along an axis of the map
        /// </summary>
        public const int Size = 256;
    
        float[] matrix;

        public Heightmap()
        {
            int n = Size + 1;
            matrix = new float[n * n];
        }

        public int Length
        {
            get { return matrix.Length; }
        }

        /// <summary>
        /// 1D index accessor
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public float this[int index]
        {
            get { return matrix[index]; }
            set { matrix[index] = value; }
        }

        /// <summary>
        /// 2D position accessor for local matrix slots
        /// </summary>
        public float this[int x, int y]
        {
            get { return matrix[x + y + (y * Size)]; }
            set { matrix[x + y + (y * Size)] = value; }
        }

        /// <summary>
        /// Use bilinear interpolation to find a height at some (x, y) within [0, 1]
        /// </summary>
        public float GetHeightBilinear(float tx, float ty)
        {
            float x = tx * (Size - 1);
            float y = ty * (Size - 1);
            int ix = (int)x;
            int iy = (int)y;

            float f00 = this[ix, iy];
            float f10 = this[ix + 1, iy];
            float f01 = this[ix, iy + 1];
            float f11 = this[ix + 1, iy + 1];

            float fractx = x - ix;
            float fracty = y - iy;

            return Mathf.Lerp(Mathf.Lerp(f00, f10, fractx), Mathf.Lerp(f01, f11, fractx), fracty);
        }

        public void SetHeight(int x, int y, float height)
        {
            this[x, y] = height;
        }
    
        /// <summary>
        /// Convert an index point to world coordinates within the given region. 
        /// Useful during linear 1D iteration of the matrix
        /// </summary>
        public Vector2 IndexToWorld(int index, Rect region)
        {
            const int n = Size + 1;
            return LocalToWorld(new Vector2(index / n, index % n) / Size, region);
        }
    
        public Vector2 LocalToWorld(Vector2 local, Rect region)
        {
            return new Vector2(
                ((region.xMax - region.xMin) * local.x) + region.xMin,
                ((region.yMax - region.yMin) * local.y) + region.yMin
            );
        }

        public Vector2 WorldToLocal(Vector2 world, Rect region)
        {
            return new Vector2(
                (world.x - region.xMin) / (region.xMax - region.xMin),
                (world.y - region.yMin) / (region.yMax - region.yMin)
            );
        }

        public void Fill(float height)
        {
            for (int i = 0; i < matrix.Length; i++)
                matrix[i] = height;
        }
    
        public Heightmap Copy()
        {
            Heightmap clone = MemberwiseClone() as Heightmap;
        
            clone.matrix = new float[matrix.Length];
            matrix.CopyTo(clone.matrix, 0);
        
            return clone;
        }
    
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    builder.Append($"{this[x, y]}, ");
                }
                builder.AppendLine();
            }
        
            return builder.ToString();
        }
    }
}
