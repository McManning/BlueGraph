
using UnityEngine;
using BlueGraph;

namespace BlueGraphExamples.Math
{
    [Node(module = "Math")]
    [NodeIcon("Icons/LessThan")]
    public class LessThan : IconNode
    {
        [Input] public float a;
        [Input] public float b;
        [Output] public bool result;

        public override object GetOutputValue(string name)
        {
            return a < b;
        }
    }
}
