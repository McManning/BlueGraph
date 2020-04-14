
using UnityEngine;
using BlueGraph;

namespace BlueGraphSamples
{
    /// <summary>
    /// Example of an execution node representing <c>for (i = 0 to count) { ... }</c>
    /// </summary>
    [Node(module = "Flow Control")]
    public class Loop : ExecNode
    {
        [Input("Count")] public int count;
        
        [Output("Current")] int m_currentCount;
        [Output("Then", multiple = false)] readonly ExecData elseExec;

        public override ICanExec Execute(ExecData data)
        {
            int count = GetInputValue("Count", this.count);
            
            // Execution does not leave this node until the loop completes
            ICanExec next = GetNextExec();
            for (m_currentCount = 0; m_currentCount < count; m_currentCount++)
            {
                (graph as ExecGraph).ExecuteSubtree(next, data);
            }
            
            return GetNextExec("Then");
        }

        public override object OnRequestValue(Port port)
        {
            if (port.name == "Count")
            {
                return m_currentCount;
            }

            return base.OnRequestValue(port);
        }
    }
}
