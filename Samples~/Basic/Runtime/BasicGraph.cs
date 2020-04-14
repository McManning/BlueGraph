
using UnityEngine;
using BlueGraph;

namespace BlueGraphSamples
{
    /// <summary>
    /// A basic BlueGraph asset. You can specify what node modules 
    /// to allow on the graph using the [IncludeModules] attribute.
    /// </summary>
    [CreateAssetMenu(menuName = "BlueGraph Samples/Basic/Basic Graph", fileName = "New BasicGraph")]
    [IncludeModules("Math", "Logic", "Debug")]
    public class BasicGraph : Graph
    {

    }
}
