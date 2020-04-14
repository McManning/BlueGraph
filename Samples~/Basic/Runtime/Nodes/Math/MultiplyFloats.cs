
using UnityEngine;
using BlueGraph;

namespace BlueGraphSamples
{
    [Node(module = "Math")]
    public class MultiplyFloats : AbstractNode
    {
        [Input] public float a;
        [Input] public float b;
        [Output("")] readonly float result;
        
        public override object OnRequestValue(Port port)
        {
            float af = GetInputValue("A", a);
            float bf = GetInputValue("B", b);

            return af * bf;
        }
    }
}
