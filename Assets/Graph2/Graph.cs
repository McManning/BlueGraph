using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Graph2
{
    [CreateAssetMenu]
    [Serializable]
    public class Graph : ScriptableObject
    {
        [SerializeField]
        [HideInInspector]
        public List<AbstractNode> Nodes = new List<AbstractNode>();

        public T AddNode<T>() where T : AbstractNode
        {
            return AddNode(typeof(T)) as T;
        }

        public virtual AbstractNode AddNode(Type type)
        {
            AbstractNode node = CreateInstance(type) as AbstractNode;
            node.Graph = this;
            node.name = type.FullName;

            Nodes.Add(node);
            return node;
        }

        public virtual void RemoveNode(AbstractNode node)
        {
            // Remove all connections to and from this node
            foreach (var port in node.Inputs)
            {
                foreach (var conn in port.Connections) 
                {
                    var output = conn.Node.GetOutputPort(conn.FieldName);
                    output.Disconnect(node, port.fieldName);
                }

                port.Connections.Clear();
            }
            
            foreach (var port in node.Outputs)
            {
                foreach (var conn in port.Connections) 
                {
                    var output = conn.Node.GetInputPort(conn.FieldName);
                    output.Disconnect(node, port.fieldName);
                }

                port.Connections.Clear();
            }
            
            Nodes.Remove(node);
        }
        
        public virtual Graph Copy()
        {
            Graph graph = Instantiate(this);

            foreach (var node in Nodes)
            {
                AbstractNode instance = Instantiate(node) as AbstractNode;
                instance.Graph = graph;
                // ports and such, private data?
            }

            return graph;
        }
    }
}
