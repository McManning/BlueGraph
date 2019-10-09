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
            public string inputGuid;
            public string inputPortName;

            public string outputGuid;
            public string outputPortName;
        }

        [Serializable]
        public class SerializedNode
        {
            public string guid;
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
            
            foreach (var element in elements)
            {
                if (element is NodeView)
                {
                    var src = (element as NodeView).NodeData;
                    var node = Instantiate(src);
                    node.name = src.name;
                    
                    // Convert connections to something that can be serialized
                    foreach (var port in node.inputs)
                    {
                        foreach (var conn in port.connections)
                        {
                            graph.m_SerializedEdges.Add(new SerializedEdge()
                            {
                                inputGuid = node.guid,
                                inputPortName = port.portName,
                                outputGuid = conn.node.guid,
                                outputPortName = conn.portName
                            });
                        }

                        port.connections.Clear();
                    }
                    
                    foreach (var port in node.outputs)
                    {
                        port.connections.Clear();
                    }
                    
                    graph.m_SerializedNodes.Add(new SerializedNode() {
                        guid = node.guid,
                        name = node.name,
                        type = node.GetType().FullName,
                        JSON = JsonUtility.ToJson(node)
                    });
                }
            }
            
            return JsonUtility.ToJson(graph);
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

            var nodeMap = new Dictionary<string, AbstractNode>();
            foreach (var node in cpGraph.m_SerializedNodes)
            {
                var instance = graph.AddNode(node.type);
                JsonUtility.FromJsonOverwrite(node.JSON, instance);
                instance.name = node.name;
                
                nodeMap.Add(node.guid, instance);
            }
            
            // Re-associate edges to the new node instances
            foreach (var edge in cpGraph.m_SerializedEdges)
            {
                if (nodeMap.ContainsKey(edge.inputGuid) && 
                    nodeMap.ContainsKey(edge.outputGuid)
                ) {
                    var inPort = nodeMap[edge.inputGuid].GetInputPort(edge.inputPortName);
                    inPort.Connect(nodeMap[edge.outputGuid], edge.outputPortName);
                }
            }

            return graph;
        }
    }
}
