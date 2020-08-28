using BlueGraph;

namespace BlueGraphSamples
{
    [Node(Path = "Math/Operator")]
    [Tags("Math")]
    public class Add : MathNode<float, float, float>
    {
        public override float Execute(float value1, float value2)
        {
            return value1 + value2;
        }
    }
}
