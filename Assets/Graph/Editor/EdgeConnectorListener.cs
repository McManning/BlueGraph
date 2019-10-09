
using UnityEngine;
using UnityEditor.Experimental.GraphView;

/// <summary>
/// Custom connector listener so that we can link up nodes and 
/// open a search box when the user drops an edge into the canvas
/// </summary>
public class EdgeConnectorListener : IEdgeConnectorListener
{   
    // References:
    // https://github.com/Unity-Technologies/ScriptableRenderPipeline/blob/dc09aba6a4cbd997f11e32a51881bf91d1b55b5e/com.unity.shadergraph/Editor/Drawing/EdgeConnectorListener.cs

    /// Provider is needed to open the search window if we drop an edge onto the canvas
    /// TODO: Maybe turn this into a delegate action instead
    SearchProvider m_SearchProvider;
    
    public EdgeConnectorListener(SearchProvider searchProvider)
    {
        m_SearchProvider = searchProvider;
    }
    
    public void OnDrop(GraphView graphView, Edge edge)
    {
        var left = edge.output;
        var right = edge.input;
        
        if (left != null && right != null)
        {
            // TODO: Register undo
            var newEdge = left.ConnectTo(right);
            graphView.AddElement(newEdge);
        }
    }

    public void OnDropOutsidePort(Edge edge, Vector2 position)
    {
        // TODO: Check if we're at least on top of another node and there's
        // some form of auto-connection on it (or any input with a matching type?)
        
        // Figure out what port we're dragging around the canvas
        // TODO: Can't I just use edge.input if edge.output is being dragged?
        Port draggedPort = null;
        if (edge.output != null)
        {
            draggedPort = edge.output.edgeConnector.edgeDragHelper.draggedPort;
        }
        else if (edge.input != null)
        {
            draggedPort = edge.input.edgeConnector.edgeDragHelper.draggedPort;
        }
        
        m_SearchProvider.connectedPort = draggedPort as NodePort;

        var ctx = new SearchWindowContext(GUIUtility.GUIToScreenPoint(
            Event.current.mousePosition
        ));
        
        SearchWindow.Open(ctx, m_SearchProvider);
    }
}
