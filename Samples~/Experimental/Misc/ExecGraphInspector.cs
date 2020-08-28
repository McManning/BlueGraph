using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using BlueGraph.Editor;

namespace BlueGraphSamples
{
   /* [CustomEditor(typeof(ExecGraph))]
    public class ExecGraphInspector : GraphEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            ExecGraph graph = target as ExecGraph;

            // Add an execute button from the inspector
            if (GUILayout.Button("Execute"))
            {
                graph.Execute();
            }
        }

        /// <summary>
        /// Override to add a custom menu bar to the window
        /// </summary>
        /// <returns></returns>
        public override GraphEditorWindow CreateEditorWindow()
        {
            var window = base.CreateEditorWindow();

            window.Canvas.AddToClassList("execCanvasView");
            window.Canvas.styleSheets.Add(Resources.Load<StyleSheet>("Styles/ExecCanvasView"));
            
            // Add a custom toolbar to the window to execute the graph
            var graph = target as ExecGraph;

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

            window.rootVisualElement.Insert(0, toolbar);

            return window;
        }
    }*/
}
