
using UnityEngine;
using BlueGraph;

namespace BlueGraphSamples
{
    [Node(Path = "Math/Boolean")]
    [Tags("Math")]
    public class Not : MathNode<bool, bool>
    {
        public override bool Execute(bool value)
        {
            return !value;
        }
    }
}
