
using System;
using System.Collections.Generic;

using UnityEngine.UIElements;
using UnityEditor.UIElements;
using BlueGraph.Editor;
using Port = BlueGraph.Port;

namespace BlueGraphSamples
{
    [CustomNodeView(typeof(ExecSubgraph))]
    public class ExecSubgraphNodeView : ExecNodeView
    {
        ObjectField m_SubgraphPicker;

        /// <summary>
        /// Add custom classes and field editors on initialize
        /// </summary>
        protected override void OnInitialize()
        {
            base.OnInitialize();


            m_SubgraphPicker = extensionContainer.ElementAt(0) as ObjectField;
            m_SubgraphPicker.RegisterValueChangedCallback((change) =>
            {
                RebuildIO(change.newValue as Subgraph);
            });
        }

        void RebuildIO(Subgraph subgraph)
        {
            this.DestroyAllPorts();
            
            if (!subgraph)
            {
                return;
            }

            // Extract IO from the subgraph and build matching ports
            var inputs = subgraph.FindNodes<InputNode>();
            var outputs = subgraph.FindNodes<OutputNode>();

            foreach (var input in inputs)
            {
                var port = new Port
                {
                    isInput = true,
                    name = input.name,
                    Type = input.GetPort("").Type
                };
                
                target.AddPort(port);
                AddInputPort(port);
            }
            
            foreach (var output in outputs)
            {
                var port = new Port
                {
                    name = output.name,
                    Type = output.GetPort("").Type,
                    acceptsMultipleConnections = true
                };
                
                target.AddPort(port);
                AddOutputPort(port);
            }
        }
    }
}
