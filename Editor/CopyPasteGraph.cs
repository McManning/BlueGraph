using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace BlueGraph.Editor
{
    /// <summary>
    /// Converts graph data into a format that can be stored on the clipboard for copy/paste
    /// </summary>
    public class CopyPasteGraph : ScriptableObject
    {
        [SerializeReference]
        public List<AbstractNode> nodes = new List<AbstractNode>();

        public List<Comment> comments = new List<Comment>();
        
        public static string Serialize(IEnumerable<GraphElement> elements)
        {
            var graph = CreateInstance<CopyPasteGraph>();

            foreach (var element in elements)
            {
                if (element is NodeView node)
                {
                    graph.nodes.Add(node.target);
                }
                else if (element is CommentView comment)
                {
                    graph.comments.Add(comment.target);
                }
            }

            string json = JsonUtility.ToJson(graph, true);
            DestroyImmediate(graph);

            return json;
        }

        public static bool CanDeserialize(string data)
        {
            try
            {
                // TODO: Verson/graph module compat testing. 
                // Otherwise someone could paste nodes into a graph that
                // doesn't have access to those particular nodes.
                var graph = CreateInstance<CopyPasteGraph>();
                JsonUtility.FromJsonOverwrite(data, graph);
                DestroyImmediate(graph);

                return true;
            } 
            catch { }
        
            return false;
        }

        public static CopyPasteGraph Deserialize(string data)
        {
            var graph = CreateInstance<CopyPasteGraph>();
            JsonUtility.FromJsonOverwrite(data, graph);
            
            // Generate new unique IDs for each node in the list
            var idMap = new Dictionary<string, string>();
            foreach (var node in graph.nodes)
            {
                var newId = Guid.NewGuid().ToString();
                idMap[node.id] = newId;
                node.id = newId;
            }

            // Remap connections to new node IDs, and drop any connections
            // that were to nodes outside of the subset of pasted nodes
            foreach (var node in graph.nodes)
            {
                foreach (var port in node.Ports)
                {
                    var edges = new List<Connection>(port.m_Connections);
                    port.m_Connections.Clear();

                    // Only re-add connections that are in the new pasted subset
                    foreach (var edge in edges)
                    {
                        if (idMap.ContainsKey(edge.nodeId))
                        {
                            port.m_Connections.Add(new Connection
                            {
                                nodeId = idMap[edge.nodeId],
                                portName = edge.portName
                            });
                        }
                    }
                }
            }
            
            return graph;
        }
    }
}
