using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using System.Linq;

/// <summary>
/// (Un)serialize elements of a graph. Supports transferring between graph <-> asset 
/// as well as handling the clipboard actions in GraphView
/// </summary>
public class GraphSerializer
{
    EdgeConnectorListener m_EdgeListener;

    public GraphSerializer(EdgeConnectorListener edgeListener)
    {
        m_EdgeListener = edgeListener;
    }
    
    public IEnumerable<GraphElement> Unserialize(string data)
    {
        var graph = JsonUtility.FromJson<SerializableGraph>(data);
        return Unserialize(graph);
    }

    public IEnumerable<GraphElement> Unserialize(SerializableGraph graph)
    {
        var nodes = new Dictionary<string, NodeView>();
        var edges = new List<GraphElement>();
        
        // Insert elements for each node
        foreach (var node in graph.Nodes) 
        {
            NodeView element = new NodeView();
            element.Initialize(node.Value, node.Value.GraphPosition, m_EdgeListener);
            
            nodes[node.Key.ToString()] = element;
        }

        // Add edge elements between new node
        foreach (var edge in graph.SerializedEdges)
        {
            nodes.TryGetValue(edge.InputNodeGuid, out NodeView input);
            nodes.TryGetValue(edge.OutputNodeGuid, out NodeView output);

            if (input == null)
            {
                Debug.LogWarning("Missing input: " + edge.InputNodeGuid);
                continue;
            }

            if (output == null)
            {
                Debug.LogWarning("Missing output: " + edge.OutputNodeGuid);
                continue;
            }
            
            var inputPort = input.GetInputPort(edge.InputPortName);
            var outputPort = output.GetOutputPort(edge.OutputPortName);
            
            edges.Add(inputPort.ConnectTo(outputPort));
        }

        return edges.Concat(nodes.Values);
    }

    public string Serialize(IEnumerable<GraphElement> elements)
    {
        var graph = CreateIndependentGraph(elements);
        return JsonUtility.ToJson(graph);
    }
    
    public SerializableGraph CreateIndependentGraph(IEnumerable<GraphElement> elements)
    {
        var graph = new SerializableGraph();
        graph.Name = "Foo bar";
        
        // Walk elements, serializing what we recognize
        foreach (var element in elements)
        {
            if (element is NodeView)
            {
                var node = element as NodeView;
                node.UpdateLinkedData();

                // TODO: These are references. I want copies.
                graph.Nodes[node.viewDataKey] = node.NodeData;
            }
        }

        // Add edges after nodes to ensure we only add edges to nodes
        // that are contained within the set
        foreach (var element in elements)
        {
            if (element is Edge)
            {
                var edge = element as Edge;
                var inputGuid = edge.input.node.viewDataKey;
                var outputGuid = edge.output.node.viewDataKey;

                if (graph.Nodes.ContainsKey(inputGuid) && graph.Nodes.ContainsKey(outputGuid))
                {
                    graph.SerializedEdges.Add(new SerializedEdge() {
                        InputNodeGuid = inputGuid,
                        InputPortName = edge.input.portName,
                        OutputNodeGuid = outputGuid,
                        OutputPortName = edge.output.portName
                    });
                } 
                else
                {
                    Debug.LogWarning("Ignoring edge " + edge.output.portName + " to " + edge.input.portName);
                    Debug.LogWarning("From " + inputGuid  + " to " + outputGuid);
                }
                
            }
        }
        
        return graph;
    }
    
    /// <summary>
    /// Check if the input JSON data can be converted into a SerializableGraph 
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool CanUnserialize(string data)
    {
        try
        {
            var graph = JsonUtility.FromJson<SerializableGraph>(data);
            return graph != null;
        } 
        catch { }
        
        return false;
    }
}
