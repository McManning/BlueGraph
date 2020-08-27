using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BlueGraph
{
    public abstract class Graph : ScriptableObject
    {
        /// <summary>
        /// Retrieve the title of the graph displayed in the editor.
        ///
        /// Override to provide identifiable information between
        /// different types of graphs (e.g. NPC AI versus Dialog Tree)
        /// </summary>
        public virtual string Title 
        {
            get { return "BLUEGRAPH"; }
        }

        /// <summary>
        /// Retrieve an enumerable of nodes on this graph
        /// </summary>
        public IReadOnlyCollection<Node> Nodes
        {
            get { return nodes.AsReadOnly(); }
        }

        [SerializeReference, HideInInspector] 
        private List<Node> nodes = new List<Node>();
        
        /// <summary>
        /// All comments to display in the editor for this Graph
        /// </summary>
        public List<Comment> Comments
        {
            get { return comments; }
        }

        [SerializeField, HideInInspector]
        private List<Comment> comments = new List<Comment>();
        
        /// <summary>
        /// Graph serialization version for safely handling automatic upgrades.
        /// </summary>
        public int AssetVersion
        {
            get { return assetVersion; }
        }

        [SerializeField, HideInInspector] 
        private int assetVersion = 1;

        /// <summary>
        /// Automatically convert nodes with a [Deprecated] attribute to their replacement.
        /// </summary>
        private void UpgradeDeprecatedNodes()
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                if (Attribute.GetCustomAttribute(nodes[i].GetType(), typeof(DeprecatedAttribute)) is DeprecatedAttribute deprecated)
                {
                    Debug.LogWarning(
                        $"{nodes[i].GetType().Name} is deprecated. " +
                        $"Upgrading to {deprecated.ReplaceWith.Name}."
                    );

                    // Gross workaround that abuses JSON serialization for recasting
                    string json = JsonUtility.ToJson(nodes[i]);
                    nodes[i] = JsonUtility.FromJson(json, deprecated.ReplaceWith) as Node;
                }
            }
        }

        /// <summary>
        /// Find a node on the Graph by unique ID 
        /// </summary>
        public Node GetNodeById(string id)
        {
            return nodes.Find((node) => node.ID == id);
        }

        /// <summary>
        /// Find the first node on the Graph of, or inherited from, the given type. 
        /// </summary>
        public T GetNode<T>() where T : Node
        {
            return nodes.Find((node) => typeof(T).IsAssignableFrom(node.GetType())) as T;
        }

        /// <summary>
        /// Find all nodes on the Graph of, or inherited from, the given type. 
        /// </summary>
        public T[] GetNodes<T>() where T : Node
        {
            var matches = new List<T>();
            for (int i = 0; i < nodes.Count; i++)
            {
                if (typeof(T).IsAssignableFrom(nodes[i].GetType()))
                {
                    matches.Add(nodes[i] as T);
                }
            }

            return matches.ToArray();
        }
        
        /// <summary>
        /// Add a new node to the Graph.
        /// 
        /// Once added, the node's <c>OnAddedToGraph()</c> method will be called.
        /// </summary>
        public void AddNode(Node node)
        {
            node.Graph = this;
            nodes.Add(node);
            node.OnAddedToGraph();
        }
        
        /// <summary>
        /// Remove a node from the Graph.
        /// 
        /// Once removed, the node's <c>OnRemovedFromGraph()</c> method will be called.
        /// </summary>
        public void RemoveNode(Node node)
        {
            node.DisconnectAllPorts();
            nodes.Remove(node);

            node.Graph = null;
            node.OnRemovedFromGraph();
        }

        /// <summary>
        /// Add a new edge between two Ports.
        /// </summary>
        public void AddEdge(Port output, Port input)
        {
            output.Connect(input);
        }

        /// <summary>
        /// Remove an edge between two Ports. 
        /// </summary>
        public void RemoveEdge(Port output, Port input)
        {
            output.Disconnect(input);
        }
    }
}
