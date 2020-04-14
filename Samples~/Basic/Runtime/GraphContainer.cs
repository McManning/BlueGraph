using UnityEngine;
using BlueGraph;

namespace BlueGraphSamples
{
    /// <summary>
    /// Example of a component that can store a reference to a graph asset
    /// </summary>
    public class GraphContainer : MonoBehaviour
    {
        public Graph graph;

        // Start is called before the first frame update
        void Start()
        {
            var node = graph?.FindNode<AddFloats>();
            Debug.Log($"Found node on graph: {node?.id}");
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
