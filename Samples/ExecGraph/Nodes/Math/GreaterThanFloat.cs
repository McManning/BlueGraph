using BlueGraph;

namespace BlueGraphExamples.ExecGraph
{
    // TODO: Does this make sense? Does it actually 
    // optimize down `>` into less instructions if we have constants?
    // I would assume so, but need to test it.
    [Node("Greater Than (float)", module = "ExecGraph/Math")]
    public class GreaterThanFloat : Operation<float, bool>
    {
        public override bool OutputOperation(float a, float b) => a > b;
        public override string CompileOperation(string a, string b) => $"{a} > {b}";
    }
}
