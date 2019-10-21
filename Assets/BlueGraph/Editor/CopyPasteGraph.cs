using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using BlueGraph;

namespace BlueGraphEditor
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
                    var src = (element as NodeView).target;
                    var node = Instantiate(src);
                    node.name = src.name;

                    // Convert connections to something that can be serialized
                    foreach (var port in node.ports)
                    {
                        // Just serialize input connections to reduce a bit of redundancy
                        if (port.isInput)
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
                        }
                       
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
            var guidMap = new Dictionary<string, string>();

            foreach (var node in cpGraph.m_SerializedNodes)
            {
                var instance = CreateInstance(node.type) as AbstractNode;
                JsonUtility.FromJsonOverwrite(node.JSON, instance);
                instance.name = node.name;
                
                nodeMap[node.guid] = instance;
                guidMap[node.guid] = instance.guid;
                graph.AddNode(instance);
                
                // Remap port associations
                foreach (var port in instance.ports)
                {
                    port.node = instance;
                }
            }
            
            // Re-associate edges to the new node instances
            foreach (var edge in cpGraph.m_SerializedEdges)
            {
                if (nodeMap.ContainsKey(edge.inputGuid) && 
                    nodeMap.ContainsKey(edge.outputGuid)
                ) {
                    var inPort = nodeMap[edge.inputGuid].GetInputPort(edge.inputPortName);
                    var outPort = nodeMap[edge.outputGuid].GetOutputPort(edge.outputPortName);

                    inPort.Connect(outPort);
                    outPort.Connect(inPort);
                }
            }

            return graph;
        }
    }
}
