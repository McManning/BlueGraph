
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using BlueGraph.Editor;
using BlueGraph;

namespace BlueGraphSamples
{
    [CustomNodeView(typeof(ExecSubgraph))]
    public class ExecSubgraphNodeView : ExecutableNodeView
    {
        private ObjectField subgraphPicker;

        /// <summary>
        /// Add custom classes and field editors on initialize
        /// </summary>
        protected override void OnInitialize()
        {
            base.OnInitialize();

            if (extensionContainer.childCount < 1)
            {
                Debug.LogError("ExecSubgraphNodeView expected an extension child");
            } 
            else
            {
                subgraphPicker = extensionContainer.ElementAt(0) as ObjectField;
                subgraphPicker.RegisterValueChangedCallback((change) =>
                {
                    RebuildIO(change.newValue as Graph);
                });
            }
        }

        void RebuildIO(Graph subgraph)
        {
            this.DestroyAllPorts();
            
            if (!subgraph)
            {
                return;
            }

            // Extract IO from the subgraph and build matching ports
            var inputs = subgraph.GetNodes<InputNode>();
            var outputs = subgraph.GetNodes<OutputNode>();

            foreach (var input in inputs)
            {
                var port = new Port
                {
                    Name = input.Name,
                    Direction = PortDirection.Input,
                    Type = input.GetPort("").Type
                };
                
                Target.AddPort(port);
                AddInputPort(port);
            }
            
            foreach (var output in outputs)
            {
                var port = new Port
                {
                    Name = output.Name,
                    Direction = PortDirection.Output,
                    Capacity = PortCapacity.Multiple,
                    Type = output.GetPort("").Type,
                };
                
                Target.AddPort(port);
                AddOutputPort(port);
            }
        }
    }
}
