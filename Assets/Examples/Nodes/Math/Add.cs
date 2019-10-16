
using UnityEngine;
using BlueGraph;

namespace BlueGraphExamples
{
    [Node("Math/Add")]
    [NodeIcon("Icons/Add")]
    public class Add : IconNode
    {
        [Input] public float a;
        [Input] public float b;
        [Output] public float result;

        public override object GetOutput(string name)
        {
            return a + b;
        }
    }
}
