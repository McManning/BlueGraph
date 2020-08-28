using UnityEngine;
using UnityEditor;
using BlueGraph;
using BlueGraph.Editor;

namespace BlueGraphSamples
{
    /// <summary>
    /// Custom inspector for state machine graphs to override CreateEditorWindow
    /// with an editor built for handling graphs containing subgraphs.
    /// </summary>
    [CustomEditor(typeof(StateMachineGraph), true)]
    public class StateMachineGraphEditor : GraphEditor
    {
        public override GraphEditorWindow CreateEditorWindow()
        {
            var window = CreateInstance<GraphWithSubgraphsEditorWindow>();
            window.Show();
            window.Load(target as Graph);
            return window;
        }
    }
}
