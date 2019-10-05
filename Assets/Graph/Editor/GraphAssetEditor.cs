using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System;

[CustomEditor(typeof(WorldgenGraph))]
public class GraphAssetEditor : Editor
{
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Edit Graph"))
        {
            ShowGraphEditor(target as WorldgenGraph);    
        }
        
        base.OnInspectorGUI();
    }
    
    private void ShowGraphEditor(WorldgenGraph graph)
    {
        // Open an editor for this graph
        GraphEditorWindow window = CreateInstance<GraphEditorWindow>();
        
        // TODO: Ensure only one window instance per-graph is open 
        
        window.Show();
        window.Load(graph);
    }
}
