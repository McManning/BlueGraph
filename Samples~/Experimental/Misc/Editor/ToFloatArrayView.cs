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
            int firstDisconnected = Inputs.FindIndex(i => !i.connected);
            if (firstDisconnected < Inputs.Count - 1)
            {
                var idx = 0;
                var toRemove = new List<PortView>();
                var type = Inputs[0].portType;

                foreach (var input in Inputs)
                {
                    Debug.Log(input.name);
                    if (input.connected)
                    {
                        // Reindex
                        input.Target.Name = $"{portName}[{idx}]";
                        input.portName = input.Target.Name;
                        idx++;
                    }
                    else
                    {
                        // Disconnected, remove port from views + model
                        toRemove.Add(input);
                        inputContainer.Remove(input);
                        Target.RemovePort(input.Target);
                    }
                }

                Inputs.RemoveAll(i => toRemove.Contains(i));

                // Add a new empty port at the end to accept new entries
                var port = new Port { 
                    Name = Inputs.Count > 0 ? "..." : $"{portName}[0]",
                    Direction = PortDirection.Input,
                    Type = type
                };
            
                Target.AddPort(port);
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
