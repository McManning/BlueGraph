using UnityEngine;
using BlueGraph;

namespace BlueGraphSamples
{
    [Node(Deletable = false)]
    [Tags("Terrain")]
    public class TerrainOutput : Node
    {
        [Input] public Heightmap map;

        public override object OnRequestValue(Port port)
        { 
            // No output ports are exposed on this node since
            // this represents the output of the Graph itself.
            return null; 
        }
    }
}
