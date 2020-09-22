using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BlueGraph
{
    public interface IGraph
    {
        T GetNode<T>() where T : Node;

        IEnumerable<T> GetNodes<T>() where T : Node;

        void AddNode(Node node);

        void RemoveNode(Node node);

        void AddEdge(Port output, Port input);

        void RemoveEdge(Port output, Port input);
    }

    public abstract class Graph : ScriptableObject, IGraph
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
        /// Retrieve the min zoom value scale used by CanvasView
        /// 
        /// Override to provide other value for the min zoom scale of the CanvasView
        /// </summary>
        public virtual float ZoomMinScale { get { return UnityEditor.Experimental.GraphView.ContentZoomer.DefaultMinScale; } }
        /// <summary>
        /// Retrieve the max zoom value scale used by CanvasView
        /// 
        /// Override to provide other value for the max zoom scale of the CanvasView
        /// </summary>
        public virtual float ZoomMaxScale { get { return UnityEditor.Experimental.GraphView.ContentZoomer.DefaultMaxScale; } }

        /// <summary>
        /// Retrieve all nodes on this graph
        /// </summary>
        public IReadOnlyList<Node> Nodes
        {
            get { return nodes.AsReadOnly(); }
        }

        [SerializeReference, HideInInspector] 
        private List<Node> nodes = new List<Node>();
        
        /// <summary>
        /// All comments to display in the editor for this Graph
        /// </summary>
        internal List<Comment> Comments
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
        /// Propagate OnDisable to all nodes.
        /// </summary>
        private void OnDisable()
        {
            OnGraphDisable();

            foreach (var node in Nodes)
            {
                node.Disable();
            }
        }
        
        /// <summary>
        /// Propagate OnEnable to all nodes.
        /// </summary>
        private void OnEnable()
        {
            OnGraphEnable();

            foreach (var node in Nodes)
            {
                node.Enable();
            }
        }
        
        /// <summary>
        /// Propagate OnValidate to all nodes.
        /// </summary>
        private void OnValidate()
        {
            OnGraphValidate();

            foreach (var node in Nodes)
            {
                node.Validate();
            }
        }
        
        /// <summary>
        /// Called during Unity's <c>OnDisable</c> event and before 
        /// <c>OnDisable</c> of all nodes on the graph.
        /// </summary>
        protected virtual void OnGraphDisable() { }

        /// <summary>
        /// Called during Unity's <c>OnEnable</c> event and before 
        /// <c>OnEnable</c> of all nodes on the graph.
        /// </summary>
        protected virtual void OnGraphEnable() { }
        
        /// <summary>
        /// Called during Unity's <c>OnValidate</c> event and before 
        /// <c>OnValidate</c> of all nodes on the graph.
        /// </summary>
        public virtual void OnGraphValidate() { }

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
          /// Find the first node on the Graph of, or inherited from, the given type. 
          /// </summary>
        public Node GetNode(Type type)
        {
            return nodes.Find((node) => type.IsAssignableFrom(node.GetType()));
        }

        /// <summary>
        /// Find all nodes on the Graph of, or inherited from, the given type. 
        /// </summary>
        public IEnumerable<T> GetNodes<T>() where T : Node
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                if (typeof(T).IsAssignableFrom(nodes[i].GetType()))
                {
                    yield return nodes[i] as T;
                }
            }
        }
        
        /// <summary>
        /// Add a new node to the Graph.
        /// 
        /// Once added, the node's <c>OnEnable()</c> method will be called.
        /// </summary>
        public void AddNode(Node node)
        {
            node.Graph = this;
            nodes.Add(node);
            node.OnAddedToGraph();
            node.OnEnable();
        }
        
        /// <summary>
        /// Remove a node from the Graph.
        /// 
        /// Once removed, the node's <c>OnDisable()</c> method will be called.
        /// </summary>
        public void RemoveNode(Node node)
        {
            node.DisconnectAllPorts();
            nodes.Remove(node);
            node.OnDisable();
            node.OnRemovedFromGraph();
            node.Graph = null;
        }

        /// <summary>
        /// Add a new edge between two Ports.
        /// </summary>
        public void AddEdge(Port output, Port input)
        {
            output.Connect(input);
            output.Node.Validate();
        }

        /// <summary>
        /// Remove an edge between two Ports. 
        /// </summary>
        public void RemoveEdge(Port output, Port input)
        {
            output.Disconnect(input);
            output.Node.Validate();
            input.Node.Validate();
        }
    }
}
