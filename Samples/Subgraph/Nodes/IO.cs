using System;
using BlueGraph;

namespace BlueGraphExamples.Subgraph
{
    [Node("Input", module = "Test/Subgraph")]
    public class GraphInput : AbstractNode
    {
        [Output] public object any;
    }

    [Node("Output", module = "Test/Subgraph")]
    public class GraphOutput : AbstractNode
    {
        [Input] public object any;
    }
}
