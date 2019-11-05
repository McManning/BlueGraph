
using BlueGraph;
using BlueGraphEditor;

namespace BlueGraphExamples
{
    [CustomNodeView(typeof(FloatOperationNode))]
    class ExampleCustomNodeView : NodeView
    {
        public override void Initialize(AbstractNode node, EdgeConnectorListener connectorListener)
        {
            base.Initialize(node, connectorListener);

            // Custom initialization logic goes here.
        }
    }
}
