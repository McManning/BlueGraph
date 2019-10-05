using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Graph2
{
    public class GraphEditorWindow : EditorWindow
    {
        Graph m_Graph;
        GraphViewElement m_GraphView;
    
        /// <summary>
        /// Load a graph asset in this window for editing
        /// </summary>
        public void Load(Graph graph)
        {
            m_Graph  = graph;

            // Add a child graph + whatever to the window and redraw
            m_GraphView = new GraphViewElement(this);
            m_GraphView.Load(graph);

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
}
