
using System;
using UnityEngine;
using BlueGraph;

namespace BlueGraphSamples
{
    [Node("Input", Path = "Subgraph")]
    [Tags("Subgraph IO")]
    public class InputNode : Node
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
            Name = "New Input";

            AddPort(new Port
            {
                Direction = PortDirection.Output,
                Capacity = PortCapacity.Multiple,
                Type = typeof(float)
            });
        }
    }
}
