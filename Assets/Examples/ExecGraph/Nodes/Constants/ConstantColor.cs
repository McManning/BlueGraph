using UnityEngine;
using BlueGraph;

namespace BlueGraphExamples.ExecGraph
{
    [Node("Constant (Color)", module = "ExecGraph/Constant")]
    public class ConstantColor : ConstantValue<Color> { }
}
