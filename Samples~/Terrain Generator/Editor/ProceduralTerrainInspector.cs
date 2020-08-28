
using UnityEditor;
using UnityEngine;
using BlueGraph;
using BlueGraph.Editor;

namespace BlueGraphSamples
{
    [CustomEditor(typeof(ProceduralTerrain))]
    public class ProceduralTerrainInspector : GraphEditor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Apply to Terrain"))
            {
                (target as ProceduralTerrain).Execute();
            }

            base.OnInspectorGUI();
        }
    }
}
