using System;
using System.Collections.Generic;
using UnityEngine;

namespace BlueGraph
{
    [Serializable]
    public abstract class Node : ISerializationCallbackReceiver
    {
        [SerializeField] string m_ID;

        public string ID {
            get { return m_ID; }
            set { m_ID = value; }
        }
        
        [SerializeField] string m_Name;

        public string Name {
            get { return m_Name; }
            set { m_Name = value; }
        }
        
        [SerializeField] Graph m_Graph;

        public Graph Graph { 
            get { return m_Graph; }
            internal set { m_Graph = value; }
        }

        [SerializeField] Vector2 m_Position;
       
        /// <summary>
        /// Where this node is located on the Graph in CanvasView
        /// </summary>
        public Vector2 Position
        {
            get { return m_Position; }
            set { m_Position = value; }
        }

        [SerializeField] List<Port> m_Ports;
        
        /// <summary>
        /// Accessor for ports and their connections to/from this node.
        /// </summary>
        public IReadOnlyCollection<Port> Ports 
        { 
            get { return m_Ports.AsReadOnly(); } 
        }

        public Node()
        {
            ID = Guid.NewGuid().ToString();
            m_Ports = new List<Port>();
        }

        public virtual void OnAfterDeserialize()
        {
            if (Graph == null)
            {
                throw new Exception(
                    $"[{Name} - {ID}] Node deserialized without a graph reference. " +
                    $"This could point to a potential memory leak"
                );
            }
            
            // Add a backref to each child port of this node.
            // We don't store this in the serialized copy to avoid cyclic refs.
            for (int i = 0; i < m_Ports.Count; i++)
            {
                m_Ports[i].Node = this;
            }
        }

        public virtual void OnBeforeSerialize() { }

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
        
        /// <summary>
        /// Get either an input or output port by name.
        /// </summary>
        public Port GetPort(string name)
        {
            return m_Ports.Find((port) => port.Name == name);
        }
        
        /// <summary>
        /// Add a new port to this node.
        /// </summary>
        public void AddPort(Port port)
        {
            var existing = GetPort(port.Name);
            if (existing != null)
            {
                throw new ArgumentException(
                    $"<b>[{Name}]</b> A port named `{port.Name}` already exists"
                );
            }

            m_Ports.Add(port);
            port.Node = this;
        }
        
        /// <summary>
        /// Remove an existing port from this node.
        /// </summary>
        public void RemovePort(Port port)
        {
            port.DisconnectAll();
            port.Node = null;

            m_Ports.Remove(port);
        }
        
        /// <summary>
        /// Safely remove every edge going in and out of this node.
        /// </summary>
        public void DisconnectAllPorts()
        {
            foreach (var port in m_Ports)
            {
                port.DisconnectAll();
            }
        }
        
        /// <summary>
        /// Get the value returned by an output port connected to the given port.
        /// 
        /// This will return <c>defaultValue</c> if the port is disconnected.
        /// </summary>
        public T GetInputValue<T>(string portName, T defaultValue = default)
        {
            var port = GetPort(portName);
            if (port == null || port.Direction == PortDirection.Output)
            {
                throw new ArgumentException(
                    $"[{Name}] No input port named `{portName}`"
                );
            }
            
            return port.GetValue(defaultValue);
        }

        /// <summary>
        /// Get a list of output values for all output ports connected
        /// to the given input port. 
        /// 
        /// This will return an empty list if the port is disconnected.
        /// </summary>
        public IEnumerable<T> GetInputValues<T>(string portName)
        {
            var port = GetPort(portName);
            if (port == null || port.Direction == PortDirection.Output)
            {
                throw new ArgumentException(
                    $"<b>[{Name}]</b> No input port named `{portName}`"
                );
            }
            
            return port.GetValues<T>();
        }

        /// <summary>
        /// Get the calculated value of a given output port.
        /// </summary>
        public T GetOutputValue<T>(string portName)
        {
            var port = GetPort(portName);
            if (port == null || port.Direction == PortDirection.Input)
            {
                throw new ArgumentException(
                    $"<b>[{Name}]</b> No output port named `{portName}`"
                );
            }

            return port.GetValue(default(T));
        }
        
        public override string ToString()
        {
            return $"{GetType()}({Name}, {ID})";
        }
    }
}
