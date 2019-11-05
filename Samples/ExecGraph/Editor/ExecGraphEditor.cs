
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using BlueGraphEditor;

namespace BlueGraphExamples.ExecGraph
{
    [CustomEditor(typeof(ExecGraph))]
    public class ExecGraphEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Edit ExecGraph"))
            {
                ShowGraphEditor();    
            }
        
            base.OnInspectorGUI();
        }
        
        private void ShowGraphEditor()
        {
            // TODO: Ensure only one window instance per-graph is open 
        
            GraphEditorWindow window = CreateInstance<GraphEditorWindow>();
            ExecGraph graph = target as ExecGraph;

            // Create a toolbar to run/compile
            var toolbar = new IMGUIContainer(() =>
            {
                GUILayout.BeginHorizontal(EditorStyles.toolbar);
                if (GUILayout.Button("Execute", EditorStyles.toolbarButton))
                {
                    graph.Execute();
                }
                
                if (GUILayout.Button("Compile", EditorStyles.toolbarButton))
                {
                    graph.Compile();
                }
                
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            });

            window.rootVisualElement.Add(toolbar);
        
            window.Show();
            window.Load(graph);
        }
    }
}
