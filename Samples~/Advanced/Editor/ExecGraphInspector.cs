
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using BlueGraph.Editor;

namespace BlueGraphSamples
{
    [CustomEditor(typeof(ExecGraph))]
    public class ExecGraphInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            ExecGraph graph = target as ExecGraph;

            if (GUILayout.Button("Edit Graph"))
            {
                ShowGraphEditor();    
            }

            if (GUILayout.Button("Execute"))
            {
                graph.Execute();
            }

            base.OnInspectorGUI();
        }
        
        /// <summary>
        /// Custom editor window setup that adds an execute button in the toolbar
        /// </summary>
        private void ShowGraphEditor()
        {
            // TODO: Ensure only one window instance per-graph is open 

            GraphEditorWindow window = CreateInstance<GraphEditorWindow>();
            ExecGraph graph = target as ExecGraph;

            // Create a toolbar to execute the graph 
            var toolbar = new IMGUIContainer(() =>
            {
                GUILayout.BeginHorizontal(EditorStyles.toolbar);
                if (GUILayout.Button("Execute", EditorStyles.toolbarButton))
                {
                    graph.Execute();
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
