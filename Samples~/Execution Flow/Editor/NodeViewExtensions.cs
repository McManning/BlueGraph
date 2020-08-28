
using System.Collections.Generic;
using BlueGraph.Editor;
using UnityEditor.Experimental.GraphView;

namespace BlueGraphSamples
{
    /// <summary>
    /// Utility extensions for NodeViews to support samples
    /// </summary>
    public static class NodeViewExtensions
    {
        /// <summary>
        /// Destroy the named input port from both the view and model
        /// </summary>
        public static void DestroyInputPort(this NodeView view, string name)
        {
            var port = view.Target.GetPort(name);
            var portView = view.GetInputPort(name);

            portView.DestroyAllEdges();

            // Remove references
            view.inputContainer.Remove(portView);
            view.Inputs.Remove(portView);
            view.Target.RemovePort(port);
        }

        /// <summary>
        /// Destroy the named output port from both the view and model
        /// </summary>
        public static void DestroyOutputPort(this NodeView view, string name)
        {
            var port = view.Target.GetPort(name);
            var portView = view.GetOutputPort(name);

            portView.DestroyAllEdges();

            // Remove references
            view.outputContainer.Remove(portView);
            view.Outputs.Remove(portView);
            view.Target.RemovePort(port);
        }

        /// <summary>
        /// Disconnect all edges from both the view and model
        /// </summary>
        public static void DestroyAllEdges(this PortView view)
        {
            // Disconnect all existing connections. 
            // This has to be done from the canvas view.
            var canvas = view.GetFirstAncestorOfType<CanvasView>();
            var edges = new List<Edge>(view.connections);

            foreach (var edge in edges) 
            {
                canvas.RemoveEdge(edge, false);
            }
        }

        /// <summary>
        /// Destroy all ports in both the view and model
        /// </summary>
        public static void DestroyAllPorts(this NodeView view)
        {
            foreach (var output in view.Outputs)
            {
                output.DestroyAllEdges();
                view.Target.RemovePort(output.Target);
            }

            foreach (var input in view.Inputs)
            {
                input.DestroyAllEdges();
                view.Target.RemovePort(input.Target);
            }

            view.outputContainer.Clear();
            view.inputContainer.Clear();

            view.Outputs.Clear();
            view.Inputs.Clear();
        }
    }
}
