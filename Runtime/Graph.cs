using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BlueGraph
{
    public class Graph : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeReference]
        public List<AbstractNode> nodes = new List<AbstractNode>();

        public List<Comment> comments = new List<Comment>();
        
        public void Awake()
        {
            Debug.Log($"[Graph] Awake");
        }

        public void OnEnable()
        {
            Debug.Log($"[Graph] Enable");
        }

        private void OnDisable()
        {
            Debug.Log($"[Graph] Disable");
        }

        public void OnAfterDeserialize()
        {
            Debug.Log($"[Graph] OnAfterDeserialize");
        }

        public void OnBeforeSerialize()
        {
            // This runs OFTEN (basically every frame) in the editor.
            // TODO: Don't do the upgrade check during this. Only on a reimport of scripts

            // Automatically upgrade anything that has been marked as 
            // deprecated before trying to persist this graph.
            // UpgradeDeprecatedNodes();
            // Debug.Log("[Graph] OnBeforeSerialize");
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
                    string json = JsonUtility.ToJson(nodes[i]);
                    nodes[i] = JsonUtility.FromJson(json, deprecated.replaceWith) as AbstractNode;
                }
            }
        }

        public AbstractNode FindNodeById(string id)
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

            for (int i = 0; i < nodes.Count; i++)
            {
                if (typeof(T).IsAssignableFrom(nodes[i].GetType()))
                {
                    matches.Add(nodes[i] as T);
                }
            }

            return matches;
        }
        
        public void AddNode(AbstractNode node)
        {
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
