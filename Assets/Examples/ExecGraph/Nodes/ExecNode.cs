
using UnityEngine;
using BlueGraph;

namespace BlueGraphExamples.ExecGraph
{
    /// <summary>
    /// Node that exposes an execution port for both IO. 
    /// Inherit to make a node executable for forward execution. 
    /// </summary>
    public class ExecNode : AbstractNode, ICanExec
    {
        [Input("_execIn", multiple = true)] public ExecData execIn;
        [Output("_execOut", multiple = false)] readonly ExecData execOut;

        /// <summary>
        /// Execute this node and return the next node to be executed.
        /// Override with your custom execution logic. 
        /// </summary>
        /// <returns></returns>
        public virtual ICanExec Execute(ExecData data)
        {
            // noop.
            return GetNextExec();
        }

        /// <summary>
        /// Get the next node that should be executed
        /// </summary>
        /// <returns></returns>
        public virtual ICanExec GetNextExec(string portName = "_execOut")
        {
            NodePort port = GetOutputPort(portName);
            if (!port.IsConnected) {
                return null;
            }
            
            if (port.connections[0].node is ICanExec node)
            {
                return node;
            }

            Debug.LogWarning(
                $"<b>[{name}]</b> Output is not an instance of ICanExec. " +
                $"Cannot execute past this point."
            );

            return null;
        }
    }
}
