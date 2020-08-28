
using UnityEngine;
using BlueGraph;

namespace BlueGraphSamples
{
    [Node(Path = "Heightmap/Factory")]
    public class NewPerlinHeightmap : HeightmapFactory
    {
        [Input] public Rect region = new Rect(0, 0, 100, 100);
        [Input] public float scale = 10f;

        public override void Execute()
        {
            Rect region = GetInputValue("region", this.region);
            float invScale = 1.0f / GetInputValue("scale", scale);

            result = new Heightmap();
            
            float ystep = region.height / Heightmap.Size;
            float xstep = region.width / Heightmap.Size;
            
            float ry = region.y;
            for (int y = 0; y <= Heightmap.Size; y++, ry += ystep)
            {
                float rx = region.x;
                for (int x = 0; x <= Heightmap.Size; x++, rx += xstep)
                { 
                    result[x, y] = Mathf.PerlinNoise(rx * invScale, ry * invScale);
                }
            }
        }
    }   
}
