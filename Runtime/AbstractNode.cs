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
    
        public Vector2 graphPosition;
        
        [SerializeField]
        List<Port> m_Ports;
        
        public IReadOnlyCollection<Port> Ports { get { return m_Ports.AsReadOnly(); } }

        public AbstractNode()
        {
            id = Guid.NewGuid().ToString();
            m_Ports = new List<Port>();

            OnRequestPorts();
        }

        public virtual void OnAfterDeserialize()
        {
            Debug.Log($"[{name} - {id}] OnAfterDeserialize");
            if (graph == null)
            {
                throw new Exception(
                    $"[{name} - {id}] Node deserialized without a graph reference. " +
                    $"This could point to a potential memory leak"
                );
            }
            
            for (int i = 0; i < m_Ports.Count; i++)
            {
                m_Ports[i].node = this;
            }

            // Add any additional ports the user wants
            OnRequestPorts();
        }

        public virtual void OnBeforeSerialize() { }

        /// <summary>
        /// Called after deserializing ports and when the node 
        /// is added to a Graph for the first time in the editor.
        /// 
        /// Override to dynamically add ports without field attributes.
        /// </summary>
        public virtual void OnRequestPorts() { } 

        /// <summary>
        /// Called when the node is added to a Graph via <c>Graph.AddNode</c>
        /// </summary>
        public virtual void OnAddedToGraph() { }

        /// <summary>
        /// Called when the node is removed from a Graph via <c>Graph.RemoveNode</c>
        /// </summary>
        public virtual void OnRemovedFromGraph() { }
    
        /// <summary>
        /// Resolve the return value associated with the given port. 
        /// </summary>
        public abstract object OnRequestValue(Port port);
        
        public Port GetPort(string portName)
        {
            return m_Ports.Find((port) => port.name == portName);
        }
        
        public void AddPort(Port port)
        {
            var existing = GetPort(port.name);
            if (existing != null)
            {
                throw new ArgumentException(
                    $"[{name}] A port named `{port.name}` already exists"
                );
            }

            m_Ports.Add(port);
            port.node = this;
        }

        public void RemovePort(Port port)
        {
            port.DisconnectAll();
            port.node = null;

            m_Ports.Remove(port);
        }
        
        public void DisconnectAllPorts()
        {
            foreach (var port in m_Ports)
            {
                port.DisconnectAll();
            }
        }
        
        public T GetInputValue<T>(string portName, T defaultValue = default)
        {
            var port = GetPort(portName);
            if (port == null || !port.isInput)
            {
                throw new ArgumentException(
                    $"[{name}] No input port named `{portName}`"
                );
            }
            
            return port.GetValue(defaultValue);
        }

        public IEnumerable<T> GetInputValues<T>(string portName)
        {
            var port = GetPort(portName);
            if (port == null || !port.isInput)
            {
                throw new ArgumentException(
                    $"[{name}] No input port named `{portName}`"
                );
            }
            
            return port.GetValues<T>();
        }

        public T GetOutputValue<T>(string portName)
        {
            var port = GetPort(portName);
            if (port == null || port.isInput)
            {
                throw new ArgumentException(
                    $"[{name}] No output port named `{portName}`"
                );
            }

            return port.GetValue(default(T));
        }
        
        public override string ToString()
        {
            return $"{GetType()}({name}, {id})";
        }
    }
}
