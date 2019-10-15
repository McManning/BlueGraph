using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BlueGraph
{
    public class Graph : ScriptableObject
    {
        [HideInInspector]
        public List<AbstractNode> nodes = new List<AbstractNode>();

        public List<NodeGroup> groups = new List<NodeGroup>();

        /*public T AddNode<T>() where T : AbstractNode
        {
            return AddNode(typeof(T)) as T;
        }*/

        public void AddNode(AbstractNode node)
        {
            node.graph = this;
            node.RegenerateGuid();
            nodes.Add(node);
        }

        public virtual AbstractNode AddNode(Type type)
        {
            AbstractNode node = CreateInstance(type) as AbstractNode;
            node.graph = this;
            node.RegenerateGuid();
            nodes.Add(node);
            return node;
        }

        public virtual AbstractNode AddNode(string type)
        {
            return AddNode(Type.GetType(type));
        }

        public virtual void RemoveNode(AbstractNode node)
        {
            // Remove all connections to and from this node
            foreach (var port in node.inputs)
            {
                foreach (var conn in port.connections) 
                {
                    var output = conn.node.GetOutputPort(conn.portName);
                    output.Disconnect(port);
                }

                port.connections.Clear();
            }
            
            foreach (var port in node.outputs)
            {
                foreach (var conn in port.connections) 
                {
                    var output = conn.node.GetInputPort(conn.portName);
                    output.Disconnect(port);
                }

                port.connections.Clear();
            }
            
            nodes.Remove(node);
        }
    }
}
