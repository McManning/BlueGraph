
using UnityEngine;
using BlueGraph;

namespace BlueGraphSamples
{
    /// <summary>
    /// 1-Dimensional Fractal Brownian Motion
    /// </summary>
    [Node("New fBM Heightmap", Path = "Heightmap/Factory")]
    public class NewfBMHeightmap : HeightmapFactory
    {
        [Input] public Rect region = new Rect(0, 0, 100, 100);
        
        [Input] public int octaves = 6;
        [Input] public float amplitude = 0.5f;
        [Input] public float lacunarity = 2f;
        [Input] public float scale = 10f;
        
        /// <summary>
        /// 2 to 1 hash function based on https://www.shadertoy.com/view/4dS3Wd
        /// </summary>
        public static float Hash21(float x, float y)
        {
            return Mathf.Repeat(
                Mathf.Sin(
                    Vector2.Dot(
                        new Vector2(x, y), 
                        new Vector2(12.9898f, 78.233f)
                    )
               ) * 43758.5453123f,
               1f
            );
        }

        /// <summary>
        /// 2D Noise function based on https://www.shadertoy.com/view/4dS3Wd
        /// </summary>
        public static float Noise(float x, float y)
        {
            float ix = Mathf.Floor(x);
            float iy = Mathf.Floor(y);
            float fx = Mathf.Repeat(x, 1f);
            float fy = Mathf.Repeat(y, 1f);
            
            // 4 corners of a 2D tile
            float a = Hash21(ix, iy);
            float b = Hash21(ix + 1f, iy);
            float c = Hash21(ix, iy + 1f);
            float d = Hash21(ix + 1f, iy + 1f);

            float ux = fx * fx * (3f - 2f * fx);
            float uy = fy * fy * (3f - 2f * fy);

            return Mathf.Lerp(a, b, ux) + 
                (c - a) * uy * (1f - ux) + 
                (d - b) * ux * uy;
        }

        public override void Execute()
        {
            Rect region = GetInputValue("region", this.region);
            int octaves = GetInputValue("octaves", this.octaves);
            float amplitude = GetInputValue("amplitude", this.amplitude);
            float lacunarity = GetInputValue("lacunarity", this.lacunarity);
            float invScale = 1f / GetInputValue("scale", scale);

            result = new Heightmap();

            const float gain = 0.5f;
        
            float ystep = region.height / Heightmap.Size;
            float xstep = region.width / Heightmap.Size;
            
            // Algorithm from https://thebookofshaders.com/13/
            float ry = region.y;
            for (int y = 0; y <= Heightmap.Size; y++, ry += ystep)
            {
                float rx = region.x;
                for (int x = 0; x <= Heightmap.Size; x++, rx += xstep)
                {
                    float fx = rx * invScale;
                    float fy = ry * invScale;
                    
                    float height = 0f;
                    float amp = amplitude;

                    for (int j = 0; j < octaves; j++) {
                        height += amp * Noise(fx, fy);
                        fx *= lacunarity;
                        fy *= lacunarity;
                        amp *= gain;
                    }

                    result[x, y] = height;
                }
            }
        }
    }
}
