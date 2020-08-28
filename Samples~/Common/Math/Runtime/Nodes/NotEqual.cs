
using UnityEngine;
using BlueGraph;

namespace BlueGraphSamples
{
    [Node(Path = "Math/Comparison")]
    [Tags("Math")]
    public class NotEqual : MathNode<float, float, bool>
    {
        public override bool Execute(float value1, float value2)
        {
            return !Mathf.Approximately(value1, value2);
        }
    }
}
