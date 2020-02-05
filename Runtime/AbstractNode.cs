using System;
using System.Collections.Generic;
using UnityEngine;

namespace BlueGraph
{
    [Serializable]
    public abstract class AbstractNode : ISerializationCallbackReceiver
    {
        public string id;
        public string name;
    
        public Graph graph;
    
        public List<Port> ports = new List<Port>();
        
        public Vector2 graphPosition;

        public AbstractNode()
        {
            id = Guid.NewGuid().ToString();
        }

        public virtual void OnAfterDeserialize()
        {
            if (graph == null)
            {
                throw new Exception(
                    $"[{name} - {id}] Node deserialized without a graph reference. " +
                    $"This could point to a potential memory leak"
                );
            }
        
            // Ports are deserialized first, and then this node.
            // Here, we assign the references back onto each port 
            // for connection lookups.
            foreach (Port port in ports)
            {
                port.node = this;
                port.graph = graph;
            }
        }

        public virtual void OnBeforeSerialize()
        {
            //Debug.Log($"Node {name} Before Serialize");
        }
    
        public virtual void OnAddedToGraph()
        {
            // Refresh port references to the graph
            foreach (Port port in ports)
            {
                port.node = this;
                port.graph = graph;
            }
        }

        public virtual void OnRemovedFromGraph()
        {

        }
    
        public Port GetPort(string portName)
        {
            return ports.Find((port) => port.name == portName);
        }

        public virtual object GetOutputValue(string portName)
        {
            // Default behaviour is "not implemented"
            Debug.LogWarning(
                $"<b>[{name}]</b> Tried to GetOutputValue of an unknown port `{portName}`. " +
                $"Returning null"
            );
            return null;
        }

        public object GetInputValue(string portName, object defaultValue = null)
        {
            Port port = GetPort(portName);
            if (port == null)
            {
                Debug.LogWarning(
                    $"<b>[{name}]</b> Tried to GetInputValue of an unknown port `{portName}`. " +
                    $"Returning default"
                );
                return defaultValue;
            }

            if (!port.IsConnected)
            {
                return defaultValue;
            }

            Port[] connections = port.ConnectedPorts;
            return connections[0].node.GetOutputValue(connections[0].name);
        }

        public T GetInputValue<T>(string portName, T defaultValue = default)
        {
            object output = GetInputValue(portName, null);
            
            if (output == null && typeof(T).IsValueType)
            {
                // Can't ChangeType a null when expecting a value type. Bail.
                Debug.LogWarning(
                    $"<b>[{name}]</b> Received null on input port `{portName}`. " +
                    $"Expected value type {typeof(T).FullName}. " +
                    $"Returning default."
                );
                return defaultValue;
            }
            
            // Short circuit Convert.ChangeType if we can cast it fast
            if (output == null || typeof(T).IsAssignableFrom(output.GetType()))
            {
                return (T)output;
            }
            
            // Try for IConvertible support
            try
            {
                return (T)Convert.ChangeType(output, typeof(T));
            }
            catch (Exception e)
            {
                Debug.LogError(
                    $"<b>[{name}]</b> Could not cast output with type `{output.GetType()}` " +
                    $"to input port `{portName}` of type `{typeof(T)}`. Error: {e}. " +
                    $"Returning default."
                );
            }

            return defaultValue;
        }
    
        public void AddPort(Port port)
        {
            Port match = ports.Find((p) => p.name == port.name);
            if (match != null)
            {
                throw new ArgumentException(
                    $"[{name}] A port named `{port.name}` already exists"
                );
            }
        
            port.node = this;
            port.graph = graph;

            ports.Add(port);
        }

        public void RemovePort(Port port)
        {
            // Cleanup the port and connections
            port.DisconnectAll();
            port.node = null;
            port.graph = null;
            
            ports.Remove(port);
        }

        public void DisconnectAllPorts()
        {
            ports.ForEach((port) => port.DisconnectAll());
        }
    }
}
