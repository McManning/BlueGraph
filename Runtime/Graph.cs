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
        /// different types of graphs (e.g. NPC AI vs Dialog Tree)
        /// </summary>
        public virtual string Title {
            get {
                return "BLUEGRAPH";
            }
        }

        /// <summary>
        /// Retrieve an enumerable of nodes on this graph
        /// </summary>
        public IReadOnlyCollection<Node> Nodes
        {
            get
            {
                return m_Nodes.AsReadOnly();
            }
        }

        /// <summary>
        /// All nodes contained within this Graph
        /// </summary>
        [SerializeReference, HideInInspector] 
        List<Node> m_Nodes = new List<Node>();

        /// <summary>
        /// All comments to display in the editor for this Graph
        /// </summary>
        [SerializeField, HideInInspector]
        protected internal List<Comment> m_Comments = new List<Comment>();

        /// <summary>
        /// Graph serialization version for safely handling automatic upgrades.
        /// </summary>
        [SerializeField, HideInInspector] 
        protected int m_Version = 1;

        /// <summary>
        /// Automatically convert nodes with a [Deprecated] attribute to their replacement.
        /// </summary>
        void UpgradeDeprecatedNodes()
        {
            for (int i = 0; i < m_Nodes.Count; i++)
            {
                if (Attribute.GetCustomAttribute(m_Nodes[i].GetType(), typeof(DeprecatedAttribute)) is DeprecatedAttribute deprecated)
                {
                    Debug.LogWarning(
                        $"{m_Nodes[i].GetType().Name} is deprecated. " +
                        $"Upgrading to {deprecated.replaceWith.Name}."
                    );

                    // Gross workaround that abuses JSON serialization for recasting
                    string json = JsonUtility.ToJson(m_Nodes[i]);
                    m_Nodes[i] = JsonUtility.FromJson(json, deprecated.replaceWith) as Node;
                }
            }
        }

        /// <summary>
        /// Find a node on the Graph by unique ID 
        /// </summary>
        public Node GetNodeById(string id)
        {
            return m_Nodes.Find((node) => node.ID == id);
        }

        /// <summary>
        /// Find the first node on the Graph of, or inherited from, the given type. 
        /// </summary>
        public T GetNode<T>() where T : Node
        {
            return m_Nodes.Find((node) => typeof(T).IsAssignableFrom(node.GetType())) as T;
        }

        /// <summary>
        /// Find all nodes on the Graph of, or inherited from, the given type. 
        /// </summary>
        public T[] GetNodes<T>() where T : Node
        {
            var matches = new List<T>();
            for (int i = 0; i < m_Nodes.Count; i++)
            {
                if (typeof(T).IsAssignableFrom(m_Nodes[i].GetType()))
                {
                    matches.Add(m_Nodes[i] as T);
                }
            }

            return matches.ToArray();
        }
        
        /// <summary>
        /// Add a new node to the Graph.
        /// 
        /// Once added, the node's <c>OnAddedToGraph()</c> method will be called.
        /// </summary>
        /// <param name="node"></param>
        public void AddNode(Node node)
        {
            node.Graph = this;
            m_Nodes.Add(node);
            node.OnAddedToGraph();
        }
        
        /// <summary>
        /// Remove a node from the Graph.
        /// 
        /// Once removed, the node's <c>OnRemovedFromGraph()</c> method will be called.
        /// </summary>
        /// <param name="node"></param>
        public void RemoveNode(Node node)
        {
            node.DisconnectAllPorts();
            m_Nodes.Remove(node);

            node.Graph = null;
            node.OnRemovedFromGraph();
        }

        /// <summary>
        /// Add a new edge between two Ports.
        /// </summary>
        /// <param name="output"></param>
        /// <param name="input"></param>
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
