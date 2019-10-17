using System;
using System.Collections.Generic;
using UnityEngine;

namespace BlueGraph
{
    [Serializable]
    public class NodePort : ISerializationCallbackReceiver
    {
        /// <summary>
        /// Distinct connection made to a NodePort
        /// </summary>
        [Serializable]
        public class Connection
        {
            /// <summary>
            /// Node this NodePort is connected to
            /// </summary>
            public AbstractNode node;
            
            /// <summary>
            /// Port on Connection.node this port is connected to
            /// </summary>
            public string portName;
        }
    
        /// <summary>
        /// Parent node of this port
        /// </summary>
        public AbstractNode node;

        /// <summary>
        /// Port name for connections
        /// </summary>
        public string portName;

        public string fieldName;
        
        public Type type;

        [SerializeField]
        string m_TypeFullName;

        /// <summary>
        /// Does this port allow multiple connections
        /// </summary>
        public bool isMulti;
        
        public bool isInput;

        /// <summary>
        /// List of current connections out of this port
        /// </summary>
        public List<Connection> connections = new List<Connection>();
    
        public void Connect(NodePort other)
        {
            Connect(other.node, other.portName);
        }

        public void Connect(AbstractNode node, string portName)
        {
            if (!IsConnected(node, portName))
            {
                connections.Add(new Connection() {
                    node = node, 
                    portName = portName
                });
            }
        } 

        public void Disconnect(NodePort other)
        {
            Disconnect(other.node, other.portName);
        }

        public void Disconnect(AbstractNode node, string portName)
        {
            connections.RemoveAll(
                (conn) => conn.node == node && conn.portName == portName
            );
        }

        public void DisconnectAll()
        {
            connections.Clear();
        }

        public bool IsConnected(AbstractNode node, string portName)
        {
            return connections.Find(
                (conn) => conn.node == node && conn.portName == portName
            ) != null;
        }

        public void OnBeforeSerialize()
        {
            m_TypeFullName = type?.AssemblyQualifiedName;
        }

        public void OnAfterDeserialize()
        {
            type = Type.GetType(m_TypeFullName);
        }
    }
}
