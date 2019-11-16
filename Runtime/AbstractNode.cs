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
        
        /// Graph metadata
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
            Debug.LogWarning(
                $"<b>[{this.name}]</b> Invalid output port `{name}`. Returning null."
            );

            return null;
        }

        /// <summary>
        /// Retrieve an input value without attempting a typecast. 
        /// </summary>
        public object GetInputValue(string name, object defaultValue = null)
        {
            var port = GetInputPort(name);
            if (port == null)
            {
                Debug.LogWarning(
                    $"<b>[{this.name}]</b> Invalid input port `{name}`. Returning default."
                );
                return defaultValue;
            }

            if (port.connections.Count < 1)
            {
                return defaultValue;
            }
            
            NodePort.Connection conn = port.connections[0];
            return conn.node.GetOutputValue(conn.portName);
        }
        
        public T GetInputValue<T>(string name, T defaultValue = default)
        {
            var port = GetInputPort(name);
            if (port == null)
            {
                Debug.LogWarning(
                    $"<b>[{this.name}]</b> Unknown port name `{name}`. Returning default."
                );
                return defaultValue;
            }

            if (port.connections.Count < 1)
            {
                return defaultValue;
            }
            
            NodePort.Connection conn = port.connections[0];
            object output = conn.node.GetOutputValue(conn.portName);
            
            if (output == null && typeof(T).IsValueType)
            {
                // Can't ChangeType a null when expecting a value type. Bail.
                Debug.LogWarning(
                    $"<b>[{this.name}]</b> Received null on input {name}. " +
                    $"Expected value type {typeof(T).FullName}. " +
                    $"Returning default."
                );
                return defaultValue;
            }
            
            // Short circuit Convert.ChangeType if it's already the expected type
            if (output == null || output.GetType() == typeof(T))
            {
                return (T)output;
            }
            
            try
            {
                return (T)Convert.ChangeType(output, typeof(T));
            }
            catch (Exception e)
            {
                Debug.LogError(
                    $"<b>[{this.name}]</b> Could not cast output `{conn.portName}` with type `{output.GetType()}` " +
                    $"to input `{this.name}` type `{typeof(T)}`. " +
                    $"Returning default."
                );
            }

            return defaultValue;
        }

        public T[] GetInputValues<T>(string name, T defaultValue = default)
        {
            List<T> values = new List<T>();
            NodePort port = GetInputPort(name);

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
