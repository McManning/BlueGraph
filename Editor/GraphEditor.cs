
using UnityEngine;
using UnityEditor;

namespace BlueGraph.Editor
{
    /// <summary>
    /// Custom inspector that adds a button to display the graph editor window for an asset.
    /// </summary>
    /// <remarks>
    /// You can inherit from this to add the basic functionality, but this is more of an
    /// example of basic setup. 
    /// </remarks>
    [CustomEditor(typeof(Graph))]
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
