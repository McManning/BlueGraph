
using UnityEngine;
using BlueGraph;

namespace BlueGraphExamples.ExecGraph
{
    /// <summary>
    /// Example of an execution node representing if (cond) { ... } else { ... }
    /// </summary>
    [Node(module = "ExecGraph")]
    public class Branch : ExecNode, ICanCompile
    {
        [Input] public bool condition;
        [Output("Else", multiple = false)] readonly ExecData elseExec;

        public override ExecNode Execute(ExecData data)
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
            NodePort input = GetInputPort("Condition");
            if (!input.IsConnected)
            {
                // Easy case: inlined constant condition
                builder.AppendLine();
                builder.AppendLine($"if ({(condition ? "true" : "false")})");
            }
            else
            {
                // Need to compile up nodes to get the condition output
                NodePort outputPort = input.GetConnection(0);
                builder.CompileInputs(outputPort);

                string variableName = builder.PortToVariable(outputPort);
                
                builder.AppendLine();
                builder.AppendLine($"if ({variableName})");
            }
            
            builder.BeginScope();

            var next = GetNextExec();
            if (next is ICanCompile ifNode)
            {
                ifNode.Compile(builder);
            }
            else
            {
                builder.AppendLine($"// TODO: Handling no ICanCompile {next?.name}");
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
