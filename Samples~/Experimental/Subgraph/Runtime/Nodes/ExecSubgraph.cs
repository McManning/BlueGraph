
using System;
using UnityEngine;
using BlueGraph;

namespace BlueGraphSamples
{
    [Node("Subgraph", Path = "Subgraph")]
    [Tags("Subgraph")]
    public class ExecSubgraph : ExecutableNode
    {
        [Editable("")] public Graph subgraph;

        public override object OnRequestValue(Port port)
        {
            // Find the SubgraphOutput node with the given name
            OutputNode output = null;

            var outputs = subgraph.GetNodes<OutputNode>();
            foreach (var node in outputs)
            {
                if (node.Name == port.Name)
                {
                    output = node;
                    break;
                }
            }

            if (output == null)
            {
                throw new Exception(
                    $"Subgraph does not contain Output node named {port.Name}"
                );
            }

            // For each input node on the subgraph, evaluate the local port
            // value of this node and cache onto the input node of the subgraph
            var inputs = subgraph.GetNodes<InputNode>();
            foreach (var input in inputs)
            {
                input.value = GetInputValue<object>(input.inputName);
            }

            // Extract the input value of the specified SubgraphOutput
            return output.GetInputValue<object>("");
        }
    }
}
