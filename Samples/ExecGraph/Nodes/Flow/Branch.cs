
using UnityEngine;
using BlueGraph;

namespace BlueGraphExamples.ExecGraph
{
    /// <summary>
    /// Example of an execution node representing if (cond) { ... } else { ... }
    /// </summary>
    [Node(module = "ExecGraph/Flow Control")]
    public class Branch : ExecNode, ICanCompile
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

        public void Compile(CodeBuilder builder)
        {
            /*
             * Build:
             * 
             * if (condition) {
             *   ifExec
             * } else {
             *   elseExec
             * }
             */
            var conditionVar = builder.PortToValue(GetInputPort("Condition"), condition);
            
            builder.AppendLine();
            builder.AppendLine($"if ({conditionVar})");
            
            builder.BeginScope();

            var next = GetNextExec();
            if (next is ICanCompile ifNode)
            {
                ifNode.Compile(builder);
            }
            else
            {
                builder.AppendLine($"// TODO: Handling no ICanCompile {(next as AbstractNode)?.name}");
            }
            
            builder.EndScope();

            // Conditionally add an else block iff there's an exec
            next = GetNextExec("Else");
            if (next is ICanCompile elseNode)
            {
                builder.AppendLine("else");

                builder.BeginScope();
                elseNode.Compile(builder);
                builder.EndScope();
            }

            builder.AppendLine();
        }
    }
}
