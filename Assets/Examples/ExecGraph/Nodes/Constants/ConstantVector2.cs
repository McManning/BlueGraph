using UnityEngine;
using BlueGraph;

namespace BlueGraphExamples.ExecGraph
{
    [Node("Constant (Vector2)", module = "ExecGraph/Constant")]
    public class ConstantVector2 : ConstantValue<Vector2> { }
}
