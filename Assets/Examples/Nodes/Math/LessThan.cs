
using UnityEngine;
using BlueGraph;

namespace BlueGraphExamples
{
    [Node("Math/Less Than")]
    [NodeIcon("Icons/LessThan")]
    public class LessThan : IconNode
    {
        [Input] public float a;
        [Input] public float b;
        [Output] public bool result;

        public override object GetOutput(string name)
        {
            return a < b;
        }
    }
}
