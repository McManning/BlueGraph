
using UnityEngine;
using UnityEditor;
using BlueGraph;

namespace BlueGraphEditor
{
    [CustomEditor(typeof(Graph))]
    public class GraphEditor : Editor
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
