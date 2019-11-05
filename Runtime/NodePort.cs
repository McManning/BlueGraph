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

        /// <summary>
        /// Underlying field name for the port. Used by the editor
        /// </summary>
        public string fieldName;
       
        /// <summary>
        /// Suggested type of this port. Used for compatibility testing
        /// </summary>
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
        
        public bool IsConnected { get { return connections.Count > 0; } }
    
        public void Connect(NodePort other)
        {
            Connect(other.node, other.portName);
        }

        public void Connect(AbstractNode node, string portName)
        {
            if (!IsConnectedTo(node, portName))
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
        
        /// <summary>
        /// Test if there's already a connection to the given node+port combination
        /// </summary>
        public bool IsConnectedTo(AbstractNode node, string portName)
        {
            return connections.Find(
                (conn) => conn.node == node && conn.portName == portName
            ) != null;
        }

        /// <summary>
        /// Retrieve the connected NodePort at the given index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public NodePort GetConnection(int index)
        {
            if (index > connections.Count - 1)
            {
                throw new IndexOutOfRangeException();
            }

            return connections[index].node.GetOutputPort(
                connections[index].portName
            );
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
