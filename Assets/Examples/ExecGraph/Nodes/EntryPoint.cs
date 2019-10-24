
using UnityEngine;
using BlueGraph;

namespace BlueGraphExamples.ExecGraph
{
    // TODO: Hide somehow. It'll be added by default to the graph
    [Node(module = "ExecGraph")]
    public class EntryPoint : ExecNode, ICanCompile
    {
        public void Compile(CodeBuilder builder)
        {
            // For an EntryPoint with inputs from the host, they'd be added here.
            builder.AppendLine("public static void Run()");
            builder.BeginScope();
          
            var next = GetNextExec();
            if (next is ICanCompile node)
            {
                node.Compile(builder);
            }
            else
            {
                builder.AppendLine($"// Not ICanCompile {next?.name}");
            }

            builder.EndScope();
        }
    }
}
