using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace BlueGraph.Editor
{
    /// <summary>
    /// Build a basic window container for the BlueGraph canvas
    /// </summary>
    public class GraphEditorWindow : EditorWindow
    {
        public CanvasView Canvas { get; protected set; }

        public Graph Graph { get; protected set; }

        /// <summary>
        /// Load a graph asset in this window for editing
        /// </summary>
        public virtual void Load(Graph graph)
        {
            Graph = graph;

            Canvas = new CanvasView(this);
            Canvas.Load(graph);
            Canvas.StretchToParentSize();
            rootVisualElement.Add(Canvas);

            titleContent = new GUIContent(graph.name);
            Repaint();
        }

        protected virtual void Update()
        {
            // Canvas can be invalidated when the Unity Editor
            // is closed and reopened with this editor window persisted.
            if (Canvas == null)
            {
                Close();
                return;
            }

            Canvas.Update();
        }

        /// <summary>
        /// Restore an already opened graph after a reload of assemblies
        /// </summary>
        protected virtual void OnEnable()
        {
            if (Graph)
            {
                Load(Graph);
            }
        }
    }
}
