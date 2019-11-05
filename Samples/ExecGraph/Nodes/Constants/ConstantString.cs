using BlueGraph;

namespace BlueGraphExamples.ExecGraph
{
    [Node("Constant (string)", module = "ExecGraph/Constant")]
    public class ConstantString : ConstantValueImpl<string> { }
}
