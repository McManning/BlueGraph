
using UnityEngine;
using UnityEngine.UIElements;
using BlueGraph;
using BlueGraphEditor;

namespace BlueGraphExamples.ExecGraph
{
    [CustomNodeView(typeof(ExecNode))]
    [CustomNodeView(typeof(ExecFuncNode))]
    class ExecNodeView : NodeView
    {
        public override void Initialize(AbstractNode node, EdgeConnectorListener connectorListener)
        {
            base.Initialize(node, connectorListener);
            
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
