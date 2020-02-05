using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace BlueGraph
{
    [Serializable]
    public class Connection
    {
        public string nodeId;
        public string portName;
    }
    
    [Serializable]
    public class Port : ISerializationCallbackReceiver
    {
        /// <summary>
        /// Display and accessor name for the port
        /// </summary>
        public string name;
    
        /// <summary>
        /// Type used for port compatibility testing within the graph editor.
        /// </summary>
        [NonSerialized] public Type type;
        
        /// <summary>
        /// Underlying field name for the port. 
        /// 
        /// Used by the editor for SerializedProperty binding
        /// </summary>
        public string fieldName;
       
        /// <summary>1
        /// True if this port can have multiple connections at once
        /// </summary>
        public bool acceptsMultipleConnections;
        
        /// <summary>
        /// True if this port accepts output connections only
        /// </summary>
        public bool isInput;
        
        [NonSerialized] public Graph graph;
        [NonSerialized] public AbstractNode node;
    
        [SerializeField] public List<Connection> connections = new List<Connection>();
        [SerializeField] string m_Type;
        
        public bool IsConnected
        {
            get
            {
                return connections.Count > 0;
            }
        }
        
        /// <summary>
        /// Retrieve a list of Ports currently connected to this Port
        /// </summary>
        public Port[] ConnectedPorts
        {
            get
            {
                var len = connections.Count;
                var ports = new Port[len];
        
                for (int i = 0; i < len; i++)
                {
                    // A graph lookup is done here instead of storing on the port in order
                    // to work around the (de)serialization order issues that Unity has 
                    // when using [SerializeReference]. Specifically:
                
                    // 1. In the current (2019.3) implementation, Unity will deserialize the Graph node list 
                    // with all nulls, then each Node is deserialized separately and add them to the graph *after* 
                    // Graph.OnAfterDeserialization(). This causes any type of node lookup done by a port 
                    // deserialization to fail.
                
                    // 2. Storing the connected nodes as a [SerializedReference] on the port introduces
                    // complexity and dependencies that break when an undo operation is performed on the graph.
                    // Referring to the graph as the source of truth reduces our complexity considerably. 
                
                    var serialized = connections[i];
                    var connected = graph.FindNodeById(serialized.nodeId); // O(1) - ideally. Currently O(Graph nodes)
                    ports[i] = connected.GetPort(serialized.portName); // O(1) - ideally. Current O(Local ports)
                }
            
                return ports;
            }
        }

        public void OnAfterDeserialize()
        {
            type = Type.GetType(m_Type);
        }
    
        public void OnBeforeSerialize()
        {
            m_Type = type?.FullName;
        }
    
        internal void Connect(Port other)
        {
            if (node == null)
            {
                throw new Exception("No node");
            }
        
            if (graph == null)
            {
                throw new Exception("No graph");
            }
        
            // TODO: Enforcement of multiple connection limits

            connections.Add(new Connection
            {
                nodeId = other.node.id,
                portName = other.name
            });
        
            other.connections.Add(new Connection
            {
                nodeId = node.id,
                portName = name
            });
        }
    
        internal void Disconnect(Port other)
        {
            // find connection and do the thing. Both sides.
            var conn = FindConnection(other);
            if (conn != null)
            {
                connections.Remove(conn);
            }
            
            var otherConn = other.FindConnection(this);
            if (otherConn != null)
            {
                other.connections.Remove(otherConn);
            }
        }
        
        internal void DisconnectAll()
        {
            // Erase our connection backref from every connected port
            var conns = ConnectedPorts;
            for (var i = 0; i < conns.Length; i++)
            {
                var backref = conns[i].FindConnection(this);
                if (backref != null)
                {
                    conns[i].connections.Remove(backref);
                }
            }

            // Finally - clear the local connection list 
            connections.Clear();
        }
        
        internal Connection FindConnection(Port other)
        {
            return connections.Find(
                (conn) => conn.nodeId == other.node.id && conn.portName == other.name
            );
        }
    }
}
