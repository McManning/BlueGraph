using BlueGraph;

namespace BlueGraphExamples.ExecGraph
{
    [Node("Divide (float)", module = "ExecGraph/Math")]
    public class DivideFloat : Operation<float, float>
    {
        public override float OutputOperation(float a, float b) => a / b;
        public override string CompileOperation(string a, string b) => $"{a} / {b}";
    }
}
