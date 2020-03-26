using System;
using System.Collections.Generic;
using UnityEngine;

namespace BlueGraph
{
    [Serializable]
    public class Connection
    {
        public string nodeId;
        public string portName;

        [NonSerialized] public Port port;
    }

    [Serializable]
    public class Port
    {
        [NonSerialized]
        public AbstractNode node;
        
        public Type Type {
            get {
                if (m_Type == null)
                {
                    m_Type = Type.GetType(m_SerializedType);
                }

                return m_Type;
            }
            set
            {
                m_Type = value;
                m_SerializedType = value?.AssemblyQualifiedName;
            }
        }
        
        public string name;
        public string fieldName;
        public bool acceptsMultipleConnections;
        public bool isInput;

        public IReadOnlyList<Port> Connections { 
            get {
                // TODO: HEAVILY optimize. Unnecessary allocations here
                LoadConnectedPorts();
                List<Port> ports = new List<Port>();
                for (var i = 0; i < m_SerializedConnections.Count; i++)
                {
                    ports.Add(m_SerializedConnections[i].port);
                }

                return ports.AsReadOnly();
            }
        }
        
        public List<Connection> SerializedConnections { 
            get {
                return m_SerializedConnections;
            }
        }
        
        [SerializeField] List<Connection> m_SerializedConnections;
        [SerializeField] string m_SerializedType;
        
        Type m_Type;

        public Port()
        {
            m_SerializedConnections = new List<Connection>();
        }
        
        public virtual T GetValue<T>(T defaultValue = default)
        {
            return defaultValue;
        }
        
        internal void DisconnectAll()
        {
            // Remove ourselves from all other connected ports
            for (var i = 0; i < m_SerializedConnections.Count; i++)
            {
                var port = m_SerializedConnections[i].port;
                port?.m_SerializedConnections.RemoveAll((edge) => edge.port == this);
            }
            
            m_SerializedConnections.Clear();
        }
        
        internal void Connect(Port port)
        {
            m_SerializedConnections.Add(new Connection() {
                port = port,
                nodeId = port.node.id,
                portName = port.name
            });

            port.m_SerializedConnections.Add(new Connection()
            {
                port = this,
                nodeId = node.id,
                portName = name
            });
        }

        // TODO: Only ever called by Graph - remove? 
        internal void Disconnect(Port port)
        {
            m_SerializedConnections.RemoveAll((edge) => edge.port == port);
            port.m_SerializedConnections.RemoveAll((edge) => edge.port == this);
        }
        
        public void LoadConnectedPorts()
        {
            var graph = node.graph;

            for (var i = 0; i < m_SerializedConnections.Count; i++)
            {
                var edge = m_SerializedConnections[i];
                var connected = graph.FindNodeById(edge.nodeId);
                edge.port = connected.GetPort(edge.portName);
                m_SerializedConnections[i] = edge;
            }
        }
    }
}
