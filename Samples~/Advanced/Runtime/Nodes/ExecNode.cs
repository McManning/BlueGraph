
using UnityEngine;
using BlueGraph;
using System.Linq;

namespace BlueGraphSamples
{
    /// <summary>
    /// Node that exposes an execution port for both IO. 
    /// Inherit to make a node executable for forward execution. 
    /// </summary>
    public abstract class ExecNode : AbstractNode, ICanExec
    {
        [Input("_execIn", multiple = true)] public ExecData execIn;
        [Output("_execOut", multiple = false)] protected readonly ExecData execOut;
        
        public override object OnRequestValue(Port port) => null;

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
        /// Get the next node that should be executed along the edge
        /// </summary>
        /// <returns></returns>
        public ICanExec GetNextExec(string portName = "_execOut")
        {
            Port port = GetPort(portName);
            if (port.TotalConnections < 1) {
                return null;
            }
            
            if (port.Connections.First() is ICanExec node)
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
