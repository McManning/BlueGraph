
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

/// <summary>
/// Required concrete implementation of a GraphView
/// </summary>
public class NodeGraphView : GraphView
{
    /// <summary>
    /// Identify all ports in the full graph that is compatible with the input.
    /// This is used for identifying whether an edge the user is dragging can be
    /// connected to some port. 
    /// </summary>
    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        var compatiblePorts = new List<Port>();
        var startNodePort = startPort as NodePort;
        
        // For the most part, we just delegate this to the NodePort to handle itself.
        // Other code samples do all the slot comparison stuff here, but it's not
        // the responsibility of GraphView to evaluate that, IMO. 
        ports.ForEach((port) => {
            var nodePort = (port as NodePort);
            if (nodePort.IsCompatibleWith(startNodePort))
            {
                compatiblePorts.Add(nodePort);
            }
        });
        
        return compatiblePorts;
    }
}
