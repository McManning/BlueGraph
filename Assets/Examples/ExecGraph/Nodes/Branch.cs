
using UnityEngine;
using BlueGraph;

namespace BlueGraphExamples.ExecGraph
{
    /// <summary>
    /// Example of an execution node representing if (cond) { ... } else { ... }
    /// </summary>
    [Node(module = "ExecGraph")]
    public class Branch : ExecNode
    {
        [Input] public bool condition;
        [Output("Else", multiple = false)] readonly ExecData elseExec;

        public override ExecNode Execute(ExecData data)
        {
            bool condition = GetInputValue("Condition", this.condition);

            if (!condition)
            {
                return GetNext("Else");
            }
            
            // True (default) case
            return base.Execute(data);
        }
    }
}
