using UnityEngine;
using BlueGraph;

namespace BlueGraphExamples.ExecGraph
{
    [Node("Constant (Vector4)", module = "ExecGraph/Constant")]
    public class ConstantVector4 : ConstantValue<Vector4> { }
}
