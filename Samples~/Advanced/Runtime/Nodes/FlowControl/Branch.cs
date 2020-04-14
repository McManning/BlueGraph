
using UnityEngine;
using BlueGraph;

namespace BlueGraphSamples
{
    /// <summary>
    /// Example of an execution node representing <c>if (cond) { ... } else { ... }</c>
    /// </summary>
    [Node(module = "Flow Control")]
    public class Branch : ExecNode
    {
        [Input] public bool condition;
        [Output("Else", multiple = false)] readonly ExecData elseExec;

        public override ICanExec Execute(ExecData data)
        {
            bool condition = GetInputValue("Condition", this.condition);

            if (!condition)
            {
                return GetNextExec("Else");
            }
            
            // True (default) case
            return base.Execute(data);
        }
    }
}
