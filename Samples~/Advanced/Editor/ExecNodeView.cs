
using UnityEngine;
using UnityEngine.UIElements;
using BlueGraph;
using BlueGraph.Editor;

namespace BlueGraphSamples
{
    [CustomNodeView(typeof(ExecNode))]
    public class ExecNodeView : NodeView
    {
        protected override void OnInitialize()
        {
            styleSheets.Add(Resources.Load<StyleSheet>("Styles/ExecNodeView"));
            AddToClassList("execNodeView");

            // Customize placement of the default exec IO ports 
            PortView inView = GetInputPort("_execIn");
            PortView outView = GetOutputPort("_execOut");

            if (inView != null) inView.AddToClassList("execInPortView");
            
            if (outView != null) outView.AddToClassList("execOutPortView");
        }
    }
}
