
using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

/// <summary>
/// Editor window for the graph.
/// </summary>
public class GraphEditorWindow : EditorWindow
{
    WorldgenGraph m_Graph;
    GraphViewElement m_GraphView;
    
    /// <summary>
    /// Load a graph asset in this window for editing
    /// </summary>
    public void Load(WorldgenGraph graph)
    {
        m_Graph = graph;
        
        // Add a child graph + whatever to the window and redraw
        m_GraphView = new GraphViewElement(this);
        m_GraphView.Load(graph.graph);
        
        // Update tab title when we have pending changes to save
        m_GraphView.onDirty = () => titleContent.text = graph.name + " *";

        // On save, apply to the underlying asset and flag it for a reload
        m_GraphView.onSave = (serialized) => {
            titleContent.text = graph.name;
            graph.graph = serialized;
            EditorUtility.SetDirty(graph);
        };
        
        rootVisualElement.Add(m_GraphView);
        
        titleContent = new GUIContent(graph.name);
        Repaint();
    }

    /// <summary>
    /// Restore an already open graph after a reload of assemblies
    /// </summary>
    private void OnEnable()
    {
        if (m_Graph)
        {
            Load(m_Graph);
        }
    }
}
