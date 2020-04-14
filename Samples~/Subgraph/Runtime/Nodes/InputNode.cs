
using System;
using UnityEngine;
using BlueGraph;

namespace BlueGraphSamples
{
    [Node("Input", module = "Subgraph/IO")]
    public class InputNode : AbstractNode
    {
        // Output is dynamically created by the editor,
        // typed to match portType with the name "Value"

        /// <summary>
        /// Value is set by a SubgraphExec prior to getting outputs
        /// </summary>
        public object value;

        public Type inputType;
        public string inputName;

        // No visible outputs, we'd evaluate the input port value.

        public override object OnRequestValue(Port port) => value;

        public InputNode()
        {
            name = "New Input";

            AddPort(new Port
            {
                acceptsMultipleConnections = true,
                Type = typeof(float)
            });
        }
    }
}
