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
        /// All nodes contained within this Graph
        /// </summary>
        [SerializeReference]
        public List<AbstractNode> nodes = new List<AbstractNode>();

        /// <summary>
        /// All comments to display in the editor for this Graph
        /// </summary>
        public List<Comment> comments = new List<Comment>();

        /// <summary>
        /// Graph serialization version for safely handling automatic upgrades.
        /// </summary>
        /*[SerializeField] int version = 1;*/

        /// <summary>
        /// Automatically convert nodes with a [Deprecated] attribute to their replacement.
        /// </summary>
        void UpgradeDeprecatedNodes()
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                if (Attribute.GetCustomAttribute(nodes[i].GetType(), typeof(DeprecatedAttribute)) is DeprecatedAttribute deprecated)
                {
                    Debug.LogWarning(
                        $"{nodes[i].GetType().Name} is deprecated. " +
                        $"Upgrading to {deprecated.replaceWith.Name}."
                    );

                    // Gross workaround that abuses JSON serialization for recasting
                    string json = JsonUtility.ToJson(nodes[i]);
                    nodes[i] = JsonUtility.FromJson(json, deprecated.replaceWith) as AbstractNode;
                }
            }
        }

        /// <summary>
        /// Find a node on the Graph by unique ID 
        /// </summary>
        public AbstractNode FindNodeById(string id)
        {
            return nodes.Find((node) => node.id == id);
        }

        /// <summary>
        /// Find the first node on the Graph of, or inherited from, the given type. 
        /// </summary>
        public T FindNode<T>() where T : AbstractNode
        {
            return nodes.Find((node) => typeof(T).IsAssignableFrom(node.GetType())) as T;
        }

        /// <summary>
        /// Find all nodes on the Graph of, or inherited from, the given type. 
        /// </summary>
        public List<T> FindNodes<T>() where T : AbstractNode
        {
            var matches = new List<T>();
            for (int i = 0; i < nodes.Count; i++)
            {
                if (typeof(T).IsAssignableFrom(nodes[i].GetType()))
                {
                    matches.Add(nodes[i] as T);
                }
            }

            return matches;
        }
        
        /// <summary>
        /// Add a new node to the Graph.
        /// 
        /// Once added, the node's <c>OnAddedToGraph()</c> method will be called.
        /// </summary>
        /// <param name="node"></param>
        public void AddNode(AbstractNode node)
        {
            node.graph = this;
            nodes.Add(node);
            node.OnAddedToGraph();
        }
        
        /// <summary>
        /// Remove a node from the Graph.
        /// 
        /// Once removed, the node's <c>OnRemovedFromGraph()</c> method will be called.
        /// </summary>
        /// <param name="node"></param>
        public void RemoveNode(AbstractNode node)
        {
            node.DisconnectAllPorts();
            nodes.Remove(node);

            node.graph = null;
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
