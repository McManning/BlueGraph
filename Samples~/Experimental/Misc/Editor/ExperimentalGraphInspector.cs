
using UnityEditor;
using UnityEngine;
using BlueGraph;
using BlueGraph.Editor;

namespace BlueGraphSamples
{
    [CustomEditor(typeof(ExperimentalGraph))]
    public class ExperimentalGraphInspector : GraphEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }

        public override GraphEditorWindow CreateEditorWindow()
        {
            var window = base.CreateEditorWindow();
            // window.Canvas.AddSearchProvider(new MyCustomProvider());
            return window;
        }
    }
}
