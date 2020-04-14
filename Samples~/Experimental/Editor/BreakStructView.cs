using UnityEngine;
using BlueGraph;
using BlueGraph.Editor;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace BlueGraphSamples
{
    /// <summary>
    /// Custom NodeView that dynamically adds/removes ports from the
    /// underlying node based on a context menu action
    /// </summary>
    [CustomNodeView(typeof(BreakStruct))]
    public class BreakStructView : NodeView
    {
        protected override void OnInitialize()
        {
            // This could also be done on AddInputPort.
            // Probably a better place so you can read type metadata as well.

            // Assuming the vec3 is the first input, add a custom
            // context menu item to break it into multiple inputs
            inputs[0].AddManipulator(
                new ContextualMenuManipulator(BuildVec3ContextualMenu)
            );
        }

        void BuildVec3ContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction(
                "Break Vector3", 
                OnBreakVector3, 
                DropdownMenuAction.AlwaysEnabled
            );
        }

        void OnBreakVector3(DropdownMenuAction action)
        {
            // Remove the Vec3 input (assuming the first)
            inputs[0].DisconnectAll();

            inputContainer.Remove(inputs[0]);
            target.RemovePort(inputs[0].target);
            inputs.Clear();

            // Add each independent input
            AddInputFloat("x");
            AddInputFloat("y");
            AddInputFloat("z");
        }

        void AddInputFloat(string name)
        {
            var port = new Port { 
                name = name,
                fieldName = name,
                isInput = true, 
                acceptsMultipleConnections = false,
                Type = typeof(float)
            };

            target.AddPort(port);
            AddInputPort(port);
        }
    }
}
