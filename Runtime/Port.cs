using System;
using System.Collections.Generic;
using UnityEngine;

namespace BlueGraph
{
    /// <summary>
    /// Serializable edge information for a Port
    /// </summary>
    [Serializable]
    public class Connection
    {
        public string nodeId;
        public string portName;

        [NonSerialized] public Port port;
    }

    /// <summary>
    /// Direction of the port
    /// </summary>
    public enum PortDirection
    {
        Input = 0,
        Output = 1
    }

    /// <summary>
    /// Number of connections that can be made to the port
    /// </summary>
    public enum PortCapacity
    {
        Single = 0,
        Multiple = 1
    }

    [Serializable]
    public class Port : ISerializationCallbackReceiver
    {
        [NonSerialized]
        public Node node;
        
        /// <summary>
        /// Display name for this port
        /// </summary>
        public string name;

        /// <summary>
        /// Whether or not multiple edges can be connected 
        /// between this port and other ports.
        /// </summary>
        public PortCapacity capacity = PortCapacity.Single;

        /// <summary>
        /// Whether to treat this as an input or output port.
        /// </summary>
        public PortDirection direction = PortDirection.Input;

        /// <summary>
        /// Allowable connection types made to this port.
        /// </summary>
        public Type type;
        
        public int ConnectionCount
        {
            get
            {
                return m_Connections.Count;
            }
        }
        
        /// <summary>
        /// Enumerate all ports connected by edges to this port
        /// </summary>
        public IEnumerable<Port> Connections { 
            get {
                HydratePorts();
                for (var i = 0; i < m_Connections.Count; i++)
                {
                    yield return m_Connections[i].port;
                }
            }
        }

        [SerializeField] internal List<Connection> m_Connections;
        [SerializeField] string m_Type;

        public Port()
        {
            m_Connections = new List<Connection>();
        }
        
        public void OnBeforeSerialize()
        {
            m_Type = type.AssemblyQualifiedName;
        }

        public void OnAfterDeserialize()
        {
            type = Type.GetType(m_Type);
        }

        /// <summary>
        /// Resolve the value on this port.
        /// 
        /// If this is an input port that accepts multiple connections,
        /// only the first connection's output value will be returned.
        /// 
        /// If this is an output port, then the node's <c>OnRequestValue()</c>
        /// will be executed and best effort will be made to convert
        /// to the requested type. 
        /// </summary>
        public virtual T GetValue<T>(T defaultValue = default)
        {
            // If this is an input port, consume the  
            // value from connected port. 
            if (direction == PortDirection.Input)
            {
                HydratePorts();
                if (m_Connections.Count > 0)
                {
                    return m_Connections[0].port.GetValue<T>();
                }

                return defaultValue;
            }
            
            // Otherwise, attempt resolution from the parent node.
            object value = node.OnRequestValue(this);

            // Make sure we don't try to cast to a value type from null
            if (value == null && typeof(T).IsValueType)
            {
                throw new InvalidCastException(
                    $"Cannot cast null to value type `{typeof(T).FullName}`"
                );
            }

            // Short circuit Convert.ChangeType if we can cast quicker
            if (value == null || typeof(T).IsAssignableFrom(value.GetType()))
            {
                return (T)value;
            }

            // Try for IConvertible support
            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch (Exception e)
            {
                throw new InvalidCastException(
                    $"Cannot cast `{value.GetType()}` to `{typeof(T)}`. Error: {e}."
                );
            }
        }

        /// <summary>
        /// Return an enumerable list of values for this port.
        /// 
        /// If this is an input port, the output value of each connected
        /// port is aggregated into a new list of values.
        /// 
        /// If this is an output port, then the node's `OnRequestValue()`
        /// will be executed with the expectation of returning IEnumerable<T>.
        /// </summary>
        public virtual IEnumerable<T> GetValues<T>()
        {
            if (direction == PortDirection.Input)
            {
                HydratePorts();

                var values = new T[m_Connections.Count];
                if (m_Connections.Count > 0)
                {
                    for (var i = 0; i < m_Connections.Count; i++)
                    {
                        values[i] = m_Connections[i].port.GetValue<T>();
                    }
                }

                return values;
            }
            
            // Otherwise, resolve from the node.
            return (IEnumerable<T>)node.OnRequestValue(this);
        }
        
        /// <summary>
        /// Remove all edges connected connected to this port.
        /// </summary>
        internal void DisconnectAll()
        {
            // Remove ourselves from all other connected ports
            for (var i = 0; i < m_Connections.Count; i++)
            {
                var port = m_Connections[i].port;
                port?.m_Connections.RemoveAll((edge) => edge.port == this);
            }
            
            m_Connections.Clear();
        }
        
        /// <summary>
        /// Add an edge between this and the given Port.
        /// 
        /// Use <c>Graph.AddEdge()</c> over this.
        /// </summary>
        internal void Connect(Port port)
        {
            // Skip if we're already connected
            if (GetConnection(port) != null) return;

            m_Connections.Add(new Connection() {
                port = port,
                nodeId = port.node.id,
                portName = port.name
            });

            port.m_Connections.Add(new Connection()
            {
                port = this,
                nodeId = node.id,
                portName = name
            });
        }

        /// <summary>
        /// Find a <c>Connection</c> to the given port if one exists.
        /// </summary>
        internal Connection GetConnection(Port port)
        {
            return m_Connections.Find((conn) => conn.port == port);
        }
        
        /// <summary>
        /// Remove any edges between this and the given Port.
        /// 
        /// Use <c>Graph.RemoveEdge()</c> over this.
        /// </summary>
        internal void Disconnect(Port port)
        {
            m_Connections.RemoveAll((edge) => edge.port == port);
            port.m_Connections.RemoveAll((edge) => edge.port == this);
        }
        
        /// <summary>
        /// Load Port class instances from the Graph for each connection.
        /// </summary>
        /// <remarks>
        /// This is implemented as an on-demand post-deserialize
        /// operation in order to avoid serializing cyclic references
        /// </remarks>
        internal void HydratePorts()
        {
            var graph = node.graph;

            for (var i = 0; i < m_Connections.Count; i++)
            {
                var edge = m_Connections[i];
                var connected = graph.FindNodeById(edge.nodeId);
                if (connected == null)
                {
                    Debug.LogWarning($"Could not locate connected node {edge.nodeId} from port {name} of {node.name}");
                }
                else
                {
                    edge.port = connected.GetPort(edge.portName);
                    m_Connections[i] = edge;
                }
            }
        }
    }
}
