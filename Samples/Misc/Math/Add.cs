
using UnityEngine;
using BlueGraph;

namespace BlueGraphExamples.Math
{
    [Node(module = "Math")]
    [NodeIcon("Icons/Add")]
    public class Add : IconNode
    {
        [Input] float a;
        [Input] float b;
        [Output] float result;

        public override object GetOutputValue(string name)
        {
            return a + b;
        }
    }
}
