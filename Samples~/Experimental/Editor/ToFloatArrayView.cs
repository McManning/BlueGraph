using UnityEngine;
using BlueGraph;
using BlueGraph.Editor;
using System.Collections.Generic;

namespace BlueGraphSamples
{
    /// <summary>
    /// Custom NodeView that dynamically adds/removes ports 
    /// from the node instance when we update connections
    /// </summary>
    [CustomNodeView(typeof(ToFloatArray))]
    public class ToFloatArrayView : NodeView
    {
        protected void UpdateArrayPort(string portName)
        {
            // TODO: Support nodes that have multiple inputs
            // that aren't just the input array.

            // TODO: Extract into an extension

            // Only reindex if we have a disconnected port in the
            // middle somewhere or we have filled up every port
            int firstDisconnected = inputs.FindIndex(i => !i.connected);
            if (firstDisconnected < inputs.Count - 1)
            {
                var idx = 0;
                var toRemove = new List<PortView>();
                var type = inputs[0].portType;

                foreach (var input in inputs)
                {
                    Debug.Log(input.name);
                    if (input.connected)
                    {
                        // Reindex
                        input.target.name = $"{portName}[{idx}]";
                        input.portName = input.target.name;
                        idx++;
                    }
                    else
                    {
                        // Disconnected, remove port from views + model
                        toRemove.Add(input);
                        inputContainer.Remove(input);
                        target.RemovePort(input.target);
                    }
                }

                inputs.RemoveAll(i => toRemove.Contains(i));

                // Add a new empty port at the end to accept new entries
                var port = new Port { 
                    name = inputs.Count > 0 ? "..." : $"{portName}[0]",
                    isInput = true, 
                    acceptsMultipleConnections = false,
                    Type = type
                };
            
                target.AddPort(port);
                AddInputPort(port);
            }
        }

        /// <summary>
        /// Override OnUpdate() to dynamically add/remove ports
        /// </summary>
        public override void OnUpdate()
        {
            UpdateArrayPort("Arr");
            base.OnDirty();
        }
    }
}
