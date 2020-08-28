
using BlueGraph;
using UnityEngine;

namespace BlueGraphSamples
{
    [Node(Path = "Math/Comparison")]
    [Tags("Math")]
    public class Max : MathNode<float, float, float>
    {
        public override float Execute(float value1, float value2)
        {
            return Mathf.Max(value1, value2);
        }
    }
}
