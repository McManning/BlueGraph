using UnityEngine;
using BlueGraph;

#if UNITY_EDITOR
using BlueGraph.Editor;
#endif 

namespace BlueGraphSamples
{
    [CreateAssetMenu(
        menuName = "BlueGraph Samples/Terrain Graph", 
        fileName = "New Terrain Graph"
    )]
    [IncludeTags("Heightmap")]
    public class TerrainGraph : Graph
    {
        #if UNITY_EDITOR
        public void OnEnable()
        {
            if (GetNode<TerrainOutput>() == null)
            {
                AddNode(NodeReflection.Instantiate<TerrainOutput>());
            }
        }
        #endif

        public Heightmap GetOutputHeightmap()
        {
            // TerrainOutput doesn't expose output ports, since it
            // should be the last node of the Graph. So instead,
            // we just read the input values directly.
            var output = GetNode<TerrainOutput>();
            var heightmap = output.GetInputValue<Heightmap>("map");
            return heightmap;
        }
    }
}
