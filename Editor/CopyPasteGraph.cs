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

            Debug.Log(json);
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
            
            // Generate unique IDs for each node and de-reference ports.

            Dictionary<string, string> remap = new Dictionary<string, string>();
            
            // Generate new unique IDs for each node in the list
            foreach (AbstractNode node in graph.nodes)
            {
                string id = node.id;
                node.id = Guid.NewGuid().ToString();
                remap[id] = node.id;
            }

            // Remap connection IDs and prune connections to nodes outside the subset
            foreach (AbstractNode node in graph.nodes)
            {
                foreach (Port port in node.ports)
                {
                    port.connections.RemoveAll(
                        (conn) => !remap.ContainsKey(conn.nodeId)
                    );

                    port.connections.ForEach(
                        (conn) => conn.nodeId = remap[conn.nodeId]
                    );
                }
            }

            return graph;
        }
    }
}
