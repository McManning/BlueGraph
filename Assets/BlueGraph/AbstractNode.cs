using System;
using System.Collections.Generic;
using UnityEngine;

namespace BlueGraph
{
    [Serializable]
    public class AbstractNode : ScriptableObject
    {
        public string guid;
        public Graph graph;
        
        public List<NodePort> ports = new List<NodePort>();
        
        // Graph metadata
        public Vector2 position;

        public void RegenerateGuid()
        {
            guid = Guid.NewGuid().ToString();
        }
        
        public NodePort GetInputPort(string name)
        {
            return ports.Find((port) => port.isInput && port.portName == name);
        }
        
        public NodePort GetOutputPort(string name)
        {
            return ports.Find((port) => !port.isInput && port.portName == name);
        }
        
        public virtual object GetOutputValue(string name)
        {
            return null;
        }

        public T GetInputValue<T>(string name, T defaultValue = default)
        {
            var port = GetInputPort(name);

            if (port != null && port.connections.Count > 0)
            {
                var conn = port.connections[0];
                return (T)conn.node.GetOutputValue(conn.portName);
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
                    (conn) => values.Add((T)conn.node.GetOutputValue(conn.portName))
                );
            }
            
            return values.ToArray();
        }

        public virtual void AddPort(NodePort port)
        {
            // TODO: Redundancy check
            ports.Add(port);
        }
    }
}
