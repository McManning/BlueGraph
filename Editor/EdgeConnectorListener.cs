
using UnityEngine;
using UnityEditor.Experimental.GraphView;

namespace BlueGraph.Editor
{
    /// <summary>
    /// Custom connector listener so that we can link up nodes and 
    /// open a search box when the user drops an edge into the canvas
    /// </summary>
    public class EdgeConnectorListener : IEdgeConnectorListener
    {
        CanvasView m_Canvas;
    
        public EdgeConnectorListener(CanvasView canvas)
        {
            m_Canvas = canvas;
        }
    
        public void OnDrop(GraphView graphView, Edge edge)
        {
            m_Canvas.AddEdge(edge);
        }

        public void OnDropOutsidePort(Edge edge, Vector2 position)
        {
            var screenPosition = GUIUtility.GUIToScreenPoint(
                Event.current.mousePosition
            );
            
            if (edge.output != null)
            {
                m_Canvas.OpenSearch(
                    screenPosition, 
                    edge.output.edgeConnector.edgeDragHelper.draggedPort as PortView
                );
            }
            else if (edge.input != null)
            {
                m_Canvas.OpenSearch(
                    screenPosition, 
                    edge.input.edgeConnector.edgeDragHelper.draggedPort as PortView
                );
            }
        }
    }
}
