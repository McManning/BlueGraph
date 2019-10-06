using System;
using System.Collections.Generic;
using UnityEngine;

namespace Graph2
{
    /// <summary>
    /// Connection information *out of* a port
    /// </summary>
    [Serializable]
    public class PortConnection
    {
        [SerializeField] public string FieldName;
        [SerializeField] public AbstractNode Node;
    }
    
    [Serializable]
    public class NodePort
    {
        [SerializeField] public bool isMulti;

        [SerializeField] public string fieldName;
        [SerializeField] public AbstractNode node;
        [SerializeField] public List<PortConnection> Connections = new List<PortConnection>();
    
        public void Connect(AbstractNode node, string port)
        {
            Connections.Add(new PortConnection() {
                Node = node, 
                FieldName = port
            });
        }    

        public void Disconnect(AbstractNode node, string port)
        {
            Connections.RemoveAll(
                (conn) => conn.Node == node && conn.FieldName == port
            );
        }

        public bool IsConnected(AbstractNode node, string port)
        {
            return Connections.Find(
                (conn) => conn.Node == node && conn.FieldName == port
            ) != null;
        }
    }
    
    [AttributeUsage(AttributeTargets.Class)]
    public class NodeAttribute : Attribute
    {
        public string Name;
    }
    
    [AttributeUsage(AttributeTargets.Field)]
    public class InputAttribute : Attribute
    {
        public string Name;
        public bool Multiple = true;
    }
    
    [AttributeUsage(AttributeTargets.Field)]
    public class OutputAttribute : Attribute
    {
        public string Name;
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class EditableAttribute : Attribute
    {
        public string Name;
    }

    [Serializable]
    public class AbstractNode : ScriptableObject
    {
        [SerializeField] public Graph Graph;

        [SerializeField] public List<NodePort> Inputs = new List<NodePort>();
        [SerializeField] public List<NodePort> Outputs = new List<NodePort>();
        
        // Graph metadata
        [SerializeField] public Vector2 position;

        // NodePortDictionary is a serializable Dictionary<string, NodePort>
        // that maps a port name to the port. Seems to only be storing dynamic
        // ports.

        public NodePort GetInputPort(string name)
        {
            return Inputs.Find((port) => port.fieldName == name);
        }
        
        public NodePort GetOutputPort(string name)
        {
            return Outputs.Find((port) => port.fieldName == name);
        }
        
        public virtual object GetOutput(string name)
        {
            // Override.
            return null; 
        }

        public T GetInputValue<T>(string name, T defaultValue = default(T))
        {
            var port = GetInputPort(name);

            if (port != null && port.Connections.Count > 0)
            {
                var conn = port.Connections[0];
                conn.Node.GetOutput(conn.FieldName);
            }

            return defaultValue;
        }
    }
}
