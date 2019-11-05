using BlueGraph;

namespace BlueGraphExamples.ExecGraph
{
    [Node("Subtract (float)", module = "ExecGraph/Math")]
    public class SubtractFloat : Operation<float, float>
    {
        public override float OutputOperation(float a, float b) => a - b;
        public override string CompileOperation(string a, string b) => $"{a} - {b}";
    }
}
