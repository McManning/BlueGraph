
using UnityEngine;
using UnityEditor;

namespace BlueGraphSamples
{
    [RequireComponent(typeof(Terrain))]
    public class ProceduralTerrain : MonoBehaviour
    {
        public TerrainGraph graph;

        /// <summary>
        /// Create a Unity Terrain, execute the graph to generate the heightmap,
        /// and assign the result to the Terrain.
        /// </summary>
        void Start()
        {
            Execute();
        }

        /// <summary>
        /// Execute the graph and apply to the Unity Terrain
        /// </summary>
        public void Execute()
        {
            if (!graph)
            {
                Debug.LogError("ProceduralTerrain needs a Graph to execute");
                return;
            }

            var terrain = GetComponent<Terrain>();
            var data = terrain.terrainData;

            var resolution = data.heightmapResolution;
            var fres = (float)resolution;

            EditorUtility.DisplayProgressBar("Generate Heightmap", "Executing Graph", 0);

            var heightmap = graph.GetOutputHeightmap();
            if (heightmap == null)
            {
                Debug.LogError("No output Heightmap");
                return;
            }

            // TODO: Faster.
            var heights = data.GetHeights(0, 0, resolution, resolution);

            for (int y = 0; y < resolution; y++)
            {
                 if (y % 20 == 0)
                 {
                     EditorUtility.DisplayProgressBar(
                         "Generate Heightmap", 
                         "Creating heightmap texture", 
                         Mathf.InverseLerp(0.0f, fres, y)
                     );
                 }

                for (int x = 0; x < resolution; x++)
                {
                    heights[y, x] = heightmap.GetHeightBilinear(x / fres, y / fres);
                }
            }
            
            EditorUtility.ClearProgressBar();

            data.SetHeights(0, 0, heights);
        }
    }
}
