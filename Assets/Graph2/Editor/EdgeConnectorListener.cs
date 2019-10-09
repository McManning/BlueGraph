
using UnityEngine;
using UnityEditor.Experimental.GraphView;

namespace Graph2
{
    /// <summary>
    /// Custom connector listener so that we can link up nodes and 
    /// open a search box when the user drops an edge into the canvas
    /// </summary>
    public class EdgeConnectorListener : IEdgeConnectorListener
    {
        GraphViewElement m_GraphView;
    
        public EdgeConnectorListener(GraphViewElement graphView)
        {
            m_GraphView = graphView;
        }
    
        public void OnDrop(GraphView graphView, Edge edge)
        {
            m_GraphView.ConnectNodes(edge);
        }

        public void OnDropOutsidePort(Edge edge, Vector2 position)
        {
            Port draggedPort = null;
            if (edge.output != null)
            {
                draggedPort = edge.output.edgeConnector.edgeDragHelper.draggedPort;
            }
            else if (edge.input != null)
            {
                draggedPort = edge.input.edgeConnector.edgeDragHelper.draggedPort;
            }
            
            Vector2 screenPosition = GUIUtility.GUIToScreenPoint(
                Event.current.mousePosition
            );
            
            m_GraphView.OpenSearch(screenPosition, draggedPort as PortView);
        }
    }
}