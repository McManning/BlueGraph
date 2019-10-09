using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Graph2
{
    /// <summary>
    /// Converts graph data into a format that can be stored on the clipboard
    /// for copy/paste actions. 
    /// </summary>
    public class CopyPasteGraph : ScriptableObject
    {
        [Serializable]
        public class SerializedEdge
        {
            public int inputID;
            public string inputPortName;

            public int outputID;
            public string outputPortName;
        }

        [Serializable]
        public class SerializedNode
        {
            public int id;
            public string name;
            public string type;
            public string JSON;
        }

        [NonSerialized]
        public List<AbstractNode> nodes = new List<AbstractNode>();

        [SerializeField]
        List<SerializedNode> m_SerializedNodes = new List<SerializedNode>();
        
        [SerializeField]
        List<SerializedEdge> m_SerializedEdges = new List<SerializedEdge>();

        public static string Serialize(IEnumerable<GraphElement> elements)
        {
            var graph = CreateInstance<CopyPasteGraph>();

            // Problem: If we serialize SO's it just serializes references.
            // This is fine for copy/paste or duplicate, but breaks for cut
            // since the referenced entity no longer exists. 

            // Either we copy the referenced into some sort of temp storage,
            // or we serialize to JSON (which doesn't seem natively supported
            // for ScriptableObjects -> JSON)

            foreach (var element in elements)
            {
                if (element is NodeView)
                {
                    var node = (element as NodeView).NodeData;
                    
                    // Convert connections to something that can be serialized
                    foreach (var port in node.inputs)
                    {
                        foreach (var conn in port.connections)
                        {
                            graph.m_SerializedEdges.Add(new SerializedEdge()
                            {
                                inputID = node.GetInstanceID(),
                                inputPortName = port.portName,
                                outputID = conn.node.GetInstanceID(),
                                outputPortName = conn.portName
                            });
                        }

                        port.connections.Clear();
                    }

                    // TODO: This copy/paste is deleting connections from the source element.
                    // But I can't instantiate() because I need the original's InstanceID().
                    // I can either copy connections to a temp storage, serialize, and then
                    // copy back or ... something else?

                    foreach (var port in node.outputs)
                    {
                        port.connections.Clear();
                    }

                    graph.m_SerializedNodes.Add(new SerializedNode() {
                        id = node.GetInstanceID(),
                        name = node.name,
                        type = node.GetType().FullName,
                        JSON = JsonUtility.ToJson(node)
                    });
                }
            }

            string data = JsonUtility.ToJson(graph);
            Debug.Log(data);
            return data;
        }

        public static bool CanDeserialize(string data)
        {
            try
            {
                var graph = CreateInstance<CopyPasteGraph>();
                JsonUtility.FromJsonOverwrite(data, graph);
                return graph.m_SerializedNodes.Count > 0;
            } 
            catch { }
        
            return false;
        }

        public static Graph Deserialize(string data)
        {
            var cpGraph = CreateInstance<CopyPasteGraph>();
            JsonUtility.FromJsonOverwrite(data, cpGraph);
            
            var graph = CreateInstance<Graph>();

            var nodeMap = new Dictionary<int, AbstractNode>();
            foreach (var node in cpGraph.m_SerializedNodes)
            {
                var instance = graph.AddNode(node.type);
                JsonUtility.FromJsonOverwrite(node.JSON, instance);
                instance.name = node.name;
                
                nodeMap.Add(node.id, instance);
            }
            
            // Re-associate edges to the new node instances
            foreach (var edge in cpGraph.m_SerializedEdges)
            {
                if (nodeMap.ContainsKey(edge.inputID) && 
                    nodeMap.ContainsKey(edge.outputID)
                ) {
                    var inPort = nodeMap[edge.inputID].GetInputPort(edge.inputPortName);
                    inPort.Connect(nodeMap[edge.outputID], edge.outputPortName);
                }
            }

            return graph;
        }
    }
}
