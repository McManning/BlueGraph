
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using BlueGraph.Editor;

namespace BlueGraphSamples
{
    [CustomNodeView(typeof(Print))]
    class PrintNodeView : NodeView
    {
        protected override void OnInitialize()
        {
            Debug.Log("<b>[PrintNodeView]</b> OnInitialize");
        }

        public override void OnUpdate()
        {
            Debug.Log("<b>[PrintNodeView]</b> OnUpdate");
        }
    }
}
