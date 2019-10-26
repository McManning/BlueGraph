using UnityEngine;
using BlueGraph;

namespace BlueGraphExamples.ExecGraph
{
    [Node("Constant (Vector3)", module = "ExecGraph/Constant")]
    public class ConstantVector3 : ConstantValue<Vector3> { }
}
