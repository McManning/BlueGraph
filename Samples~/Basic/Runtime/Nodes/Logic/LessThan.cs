
using UnityEngine;
using BlueGraph;

namespace BlueGraphSamples
{
    [Node(module = "Logic")]
    public class LessThan : AbstractNode
    {
        [Input] public float a;
        [Input] public float b;
        [Output] public bool result;
        
        public override object OnRequestValue(Port port)
        {
            return a < b;
        }
    }
}
