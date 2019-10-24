
using UnityEngine;
using BlueGraph;

namespace BlueGraphExamples.ExecGraph
{
    /// <summary>
    /// Example of an execution node representing for (i = 0 to Count) { ... }
    /// </summary>
    [Node(module = "ExecGraph")]
    public class Loop : ExecNode, ICanCompile
    {
        [Input] public int count;
        
        [Output("Count")] int m_currentCount;
        [Output("Then", multiple = false)] readonly ExecData elseExec;

        public override ExecNode Execute(ExecData data)
        {
            int count = GetInputValue("Count", this.count);

            // Execution does not leave this node until the loop completes.
            // Not sure if I like this idea, but it's the simplest version.
            // This implies we repeat the code from ExecGraph.Execute though
            for (m_currentCount = 0; m_currentCount < count; m_currentCount++)
            {
                ExecNode next = GetNextExec();
                while (next)
                {
                    next = next.Execute(data);
                }
            }
            
            return GetNextExec("Then");
        }

        public override object GetOutputValue(string name)
        {
            if (name == "Count")
            {
                return m_currentCount;
            }

            return base.GetOutputValue(name);
        }
        
        /// <param name="builder"></param>
        public void Compile(CodeBuilder builder)
        {
            /*
             * Build:
             * 
             * for (int i = 0; i < count; i++) {
             *   loopExec
             * }
             * 
             * thenExec
             */

            // Get either a constant value for the count or the input variable
            // if the port has a connection 
            NodePort countPort = GetInputPort("Count");
            if (countPort.IsConnected)
            {
                NodePort outputPort = countPort.GetConnection(0);
                builder.CompileInputs(outputPort);

                string variableName = builder.PortToVariable(outputPort);

                builder.AppendLine();
                builder.AppendLine($"for (int i = 0; i < {variableName}; i++)");
            } 
            else
            {
                // Constant loop count
                builder.AppendLine();
                builder.AppendLine($"for (int i = 0; i < {count}; i++)");
            }
            
            // Add the loop body in scope
            builder.BeginScope();

            var loopExec = GetNextExec();
            if (loopExec is ICanCompile loopNode)
            {
                loopNode.Compile(builder);
            }
            else
            {
                builder.AppendLine($"// TODO: Handling no ICanCompile {loopExec?.name}");
            }
            
            builder.EndScope();
            builder.AppendLine();

            // Add the after-loop code
            var thenExec = GetNextExec("Then");
            if (thenExec is ICanCompile thenNode)
            {
                thenNode.Compile(builder);
            }
        }
    }
}
