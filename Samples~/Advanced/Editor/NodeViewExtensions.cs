
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
            var port = view.target.GetPort(name);
            var portView = view.GetInputPort(name);

            portView.DestroyAllEdges();

            // Remove references
            view.inputContainer.Remove(portView);
            view.inputs.Remove(portView);
            view.target.RemovePort(port);
        }

        /// <summary>
        /// Destroy the named output port from both the view and model
        /// </summary>
        public static void DestroyOutputPort(this NodeView view, string name)
        {
            var port = view.target.GetPort(name);
            var portView = view.GetOutputPort(name);

            portView.DestroyAllEdges();

            // Remove references
            view.outputContainer.Remove(portView);
            view.outputs.Remove(portView);
            view.target.RemovePort(port);
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
            foreach (var output in view.outputs)
            {
                output.DestroyAllEdges();
                view.target.RemovePort(output.target);
            }

            foreach (var input in view.inputs)
            {
                input.DestroyAllEdges();
                view.target.RemovePort(input.target);
            }

            view.outputContainer.Clear();
            view.inputContainer.Clear();

            view.outputs.Clear();
            view.inputs.Clear();
        }
    }
}
