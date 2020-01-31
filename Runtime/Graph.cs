using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BlueGraph
{
    public class Graph : ScriptableObject, ISerializationCallbackReceiver
    {
        public string displayName;
    
        [SerializeReference]
        public List<AbstractNode> nodes = new List<AbstractNode>();

        public List<Comment> comments = new List<Comment>();

        public void Awake()
        {
            displayName = "Foo Graph";
        
            Debug.Log("Graph Awake");
            Debug.Log("A > Count: " + nodes.Count);
        
            if (nodes.Count > 0)
                Debug.Log("A > First item: " + nodes[0]);
        }

        public void OnEnable()
        {
            Debug.Log("Graph Enable");
            Debug.Log("E > Count: " + nodes.Count);
        
            if (nodes.Count > 0)
                Debug.Log("E > First item: " + nodes[0]);
        }
    
        private void OnDisable()
        {
            Debug.Log("Graph Disable");
            Debug.Log("D > Count: " + nodes.Count);
        
            if (nodes.Count > 0)
                Debug.Log("D > First item: " + nodes[0]);
        }

        public void OnAfterDeserialize()
        {

        }

        public void OnBeforeSerialize()
        {
            // Automatically upgrade anything that has been marked as 
            // deprecated before trying to persist this graph.
            UpgradeDeprecatedNodes();
        }
    
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
                    var json = JsonUtility.ToJson(nodes[i]);
                    nodes[i] = JsonUtility.FromJson(json, deprecated.replaceWith) as AbstractNode;
                }
            }
        }
    
        public AbstractNode FindNodeById(int id)
        {
            return nodes.Find((node) => node.id == id);
        }

        public T FindNode<T>() where T : AbstractNode
        {
            return nodes.Find((node) => typeof(T).IsAssignableFrom(node.GetType())) as T;
        }

        public List<T> FindNodes<T>() where T : AbstractNode
        {
            // TODO: More performant solution?
            var matches = new List<T>();

            for (var i = 0; i < nodes.Count; i++)
            {
                if (typeof(T).IsAssignableFrom(nodes[i].GetType()))
                {
                    matches.Add(nodes[i] as T);
                }
            }

            return matches;
        }
    
        public int GetUniqueNodeId()
        {
            int id = 0;
            nodes.ForEach((node) => id = Math.Max(id, node.id));
            return id + 1;
        }
    
        public void AddNode(AbstractNode node)
        { 
            node.id = GetUniqueNodeId();
            node.graph = this;
        
            nodes.Add(node);
            node.OnAddedToGraph();
        }
    
        public void AddEdge(Port output, Port input)
        {
            output.Connect(input);
        }
    
        public void RemoveEdge(Port output, Port input)
        {
            output.Disconnect(input);
        }

        public void RemoveNode(AbstractNode node)
        {
            node.DisconnectAllPorts();
            nodes.Remove(node);

            node.graph = null;
            node.OnRemovedFromGraph();
        }
    }
}
