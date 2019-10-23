
using UnityEngine;
using BlueGraph;

namespace BlueGraphExamples.Math
{
    [Node(module = "Math")]
    [NodeIcon(showTitle = true)]
    public class Multiply : IconNode
    {
        [Input] public float a;
        [Input] public float b;
        [Output] public float result;

        public override object GetOutputValue(string name)
        {
            return a * b;
        }
    }
}
