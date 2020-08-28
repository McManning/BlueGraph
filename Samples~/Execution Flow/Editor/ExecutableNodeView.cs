
using UnityEngine;
using UnityEngine.UIElements;
using BlueGraph;
using BlueGraph.Editor;

namespace BlueGraphSamples
{
    [CustomNodeView(typeof(ExecutableNode))]
    [CustomNodeView(typeof(EventNode))]
    public class ExecutableNodeView : NodeView
    {
        protected override void OnInitialize()
        {
            base.OnInitialize();

            styleSheets.Add(Resources.Load<StyleSheet>("Styles/ExecutableNodeView"));
            AddToClassList("executableNodeView");

            if (Target is EventNode)
            {
                AddToClassList("eventNodeView");
            }

            // Customize placement of the default exec IO ports 
            PortView inView = GetInputPort("ExecIn");
            PortView outView = GetOutputPort("ExecOut");

            if (inView != null) inView.AddToClassList("execInPortView");
            
            if (outView != null) outView.AddToClassList("execOutPortView");
        }
    }
}
