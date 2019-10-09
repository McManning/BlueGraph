using System;
using System.Collections.Generic;
using UnityEngine;

namespace Graph2
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
