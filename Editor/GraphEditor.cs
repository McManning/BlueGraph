
using UnityEngine;
using UnityEditor;

namespace BlueGraph.Editor
{
    /// <summary>
    /// Basic inspector that adds a button to edit the graph.
    /// 
    /// Typically, you should build your own inspectors that
    /// open an instance of GraphEditorWindow for the asset.
    /// </summary>
    [CustomEditor(typeof(Graph), true)]
    public class GraphEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Edit Graph"))
            {
                ShowGraphEditor();    
            }
        
            base.OnInspectorGUI();
        }
        
        private void ShowGraphEditor()
        {
            // Open an editor for this graph
            GraphEditorWindow window = CreateInstance<GraphEditorWindow>();

            // TODO: Ensure only one window instance per-graph is open 
        
            window.Show();
            window.Load(target as Graph);
        }
    }
}
