
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using BlueGraph.Editor;

namespace BlueGraphSamples
{
    [CustomEditor(typeof(DialogGraph))]
    public class DialogGraphInspector : GraphEditor
    {
        public override GraphEditorWindow CreateEditorWindow()
        {
            var window = base.CreateEditorWindow();

            window.Canvas.AddToClassList("dialogCanvasView");
            window.Canvas.styleSheets.Add(Resources.Load<StyleSheet>("Styles/DialogCanvasView"));
            
            return window;
        }
    }
}
