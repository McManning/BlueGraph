
using UnityEngine;
using BlueGraph;

namespace BlueGraphSamples
{
    [Node(Path = "Heightmap/Factory")]
    public class NewFlatHeightmap : HeightmapFactory
    {
        [Input] public float height;

        public override void Execute()
        {
            float height = GetInputValue("height", this.height);
            
            result = new Heightmap();
            result.Fill(height);
        }
    }
}
