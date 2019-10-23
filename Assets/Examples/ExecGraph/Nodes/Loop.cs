
using UnityEngine;
using BlueGraph;

namespace BlueGraphExamples.ExecGraph
{
    /// <summary>
    /// Example of an execution node representing for (i = 0 to Count) { ... }
    /// </summary>
    [Node(module = "ExecGraph")]
    public class Loop : ExecNode
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
                ExecNode next = GetNext();
                while (next)
                {
                    next = next.Execute(data);
                }
            }
            
            return GetNext("Then");
        }

        public override object GetOutputValue(string name)
        {
            if (name == "Count")
            {
                return m_currentCount;
            }

            return base.GetOutputValue(name);
        }
    }
}
