
using System;
using UnityEngine;
using BlueGraph;

namespace BlueGraphSamples
{
    [Node("Subgraph", module = "Subgraph")]
    public class ExecSubgraph : ExecNode
    {
        [Editable("")] public Subgraph subgraph;

        public override object OnRequestValue(Port port)
        {
            // Find the SubgraphOutput node with the given name
            OutputNode output = null;

            var outputs = subgraph.FindNodes<OutputNode>();
            foreach (var node in outputs)
            {
                if (node.name == port.name)
                {
                    output = node;
                    break;
                }
            }

            if (output == null)
            {
                throw new Exception(
                    $"Subgraph does not contain Output node named {port.name}"
                );
            }

            // For each input node on the subgraph, evaluate the local port
            // value of this node and cache onto the input node of the subgraph
            var inputs = subgraph.FindNodes<InputNode>();
            foreach (var input in inputs)
            {
                input.value = GetInputValue<object>(input.inputName);
            }

            // Extract the input value of the specified SubgraphOutput
            return output.GetInputValue<object>("");
        }
    }
}
