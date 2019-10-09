using System;
using System.Collections.Generic;
using UnityEngine;

namespace Graph2
{
    [Serializable]
    public class NodeGroup
    {
        public string title;
        public List<AbstractNode> nodes = new List<AbstractNode>();
    }

    /// <summary>
    /// Connection information *out of* a port
    /// </summary>
    [Serializable]
    public class PortConnection
    {
        public AbstractNode node;
        public string portName;
    }
    
    [Serializable]
    public class NodePort
    {
        public string portName;
        public AbstractNode node;
        public bool isMulti;

        public List<PortConnection> connections = new List<PortConnection>();
    
        public void Connect(AbstractNode node, string portName)
        {
            connections.Add(new PortConnection() {
                node = node, 
                portName = portName
            });
        }    

        public void Disconnect(AbstractNode node, string portName)
        {
            connections.RemoveAll(
                (conn) => conn.node == node && conn.portName == portName
            );
        }

        public bool IsConnected(AbstractNode node, string portName)
        {
            return connections.Find(
                (conn) => conn.node == node && conn.portName == portName
            ) != null;
        }
    }
    
    [AttributeUsage(AttributeTargets.Class)]
    public class NodeAttribute : Attribute
    {
        public string Name;

        public NodeAttribute(string name = null)
        {
            Name = name;
        }
    }
    
    [AttributeUsage(AttributeTargets.Field)]
    public class InputAttribute : Attribute
    {
        public string Name;
        public bool Multiple = true;
        public bool Editable = true;
        
        public InputAttribute(string name = null)
        {
            Name = name;
        }
    }
    
    [AttributeUsage(AttributeTargets.Field)]
    public class OutputAttribute : Attribute
    {
        public string Name;
        
        public OutputAttribute(string name = null)
        {
            Name = name;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class EditableAttribute : Attribute
    {
        public string Name;
        
        public EditableAttribute(string name = null)
        {
            Name = name;
        }
    }

    [Serializable]
    public class AbstractNode : ScriptableObject
    {
        public string guid;
        public Graph graph;
        public List<NodePort> inputs = new List<NodePort>();
        public List<NodePort> outputs = new List<NodePort>();
        
        // Graph metadata
        public Vector2 position;

        // NodePortDictionary is a serializable Dictionary<string, NodePort>
        // that maps a port name to the port. Seems to only be storing dynamic
        // ports.

        public void RegenerateGuid()
        {
            guid = Guid.NewGuid().ToString();
        }
        
        public NodePort GetInputPort(string name)
        {
            return inputs.Find((port) => port.portName == name);
        }
        
        public NodePort GetOutputPort(string name)
        {
            return outputs.Find((port) => port.portName == name);
        }
        
        public virtual object GetOutput(string name)
        {
            // Override.
            return null; 
        }

        public T GetInputValue<T>(string name, T defaultValue = default)
        {
            var port = GetInputPort(name);

            if (port != null && port.connections.Count > 0)
            {
                var conn = port.connections[0];
                conn.node.GetOutput(conn.portName);
            }

            return defaultValue;
        }

        public T[] GetInputValues<T>(string name, T defaultValue = default)
        {
            var values = new List<T>();
            var port = GetInputPort(name);

            if (port.connections.Count < 1 && defaultValue != null)
            {
                values.Add(defaultValue);
            }
            else
            {
                port.connections.ForEach(
                    (conn) => values.Add((T)conn.node.GetOutput(conn.portName))
                );
            }
            
            return values.ToArray();
        }
    }
}
