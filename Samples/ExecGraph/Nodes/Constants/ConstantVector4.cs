using UnityEngine;
using BlueGraph;

namespace BlueGraphExamples.ExecGraph
{
    [Node("Constant (Vector4)", module = "ExecGraph/Constant")]
    public class ConstantVector4 : ConstantValueImpl<Vector4> { }
}
