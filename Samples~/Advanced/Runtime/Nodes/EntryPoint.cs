
using UnityEngine;
using BlueGraph;

namespace BlueGraphSamples
{
    /// <summary>
    /// Node that represents an entry point into an ExecGraph.
    /// </summary>
    [Node(module = "Hidden")] // TODO: Some way to prevent deletion
    public class EntryPoint : ExecNode 
    { 
        public EntryPoint(): base()
        {
            // Since this node is created via `new` within an ExecGraph,
            // we need to manually insert the expected ports/metadata 
            // that would be typically added via editor reflection.
            name = "Entry Point";

            AddPort(new Port
            {
                name = "_execOut",
                fieldName = "execOut",
                Type = typeof(ExecData),
                isInput = false,
                acceptsMultipleConnections = false
            });
        }
    }
}
