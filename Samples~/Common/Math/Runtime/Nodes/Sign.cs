
using UnityEngine;
using BlueGraph;

namespace BlueGraphSamples
{
    [Node(Path = "Math/Operation")]
    [Tags("Math")]
    public class Sign : MathNode<float, float>
    {
        public override float Execute(float value)
        {
            return Mathf.Sign(value);
        }
    }
}
