
using UnityEditor;
using UnityEngine;
using BlueGraph;

namespace BlueGraphEditor
{
    /// <summary>
    /// Build a basic window container for the BlueGraph canvas
    /// </summary>
    public class GraphEditorWindow : EditorWindow
    {
        Graph m_Graph;
        CanvasView m_Canvas;
    
        /// <summary>
        /// Load a graph asset in this window for editing
        /// </summary>
        public void Load(Graph graph)
        {
            m_Graph  = graph;
            
            m_Canvas = new CanvasView(this);
            m_Canvas.Load(graph);
            
            rootVisualElement.Add(m_Canvas);
        
            titleContent = new GUIContent(graph.name);
            Repaint();
        }

        private void Update()
        {
            m_Canvas.Update();
        }

        /// <summary>
        /// Restore an already opened graph after a reload of assemblies
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
