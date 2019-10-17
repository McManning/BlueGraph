
using UnityEngine;
using BlueGraph;

namespace BlueGraphExamples.Math
{
    [Node(category = "Math")]
    [NodeIcon("Icons/Multiply")]
    public class Multiply : IconNode
    {
        [Input] public float a;
        [Input] public float b;
        [Output] public float result;

        public override object GetOutput(string name)
        {
            return a * b;
        }
    }
}
