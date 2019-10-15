using System;
using System.Collections.Generic;

namespace BlueGraph
{
    [Serializable]
    public class NodePort
    {
        /// <summary>
        /// Connection information *out of* a port
        /// TODO: May be able to remove this if we link nodeports together directly.
        /// The only benefit I see of this is additional edge data (reroute notes)
        /// </summary>
        [Serializable]
        public class Connection
        {
            public AbstractNode node;
            public string portName;
        }
    
        public AbstractNode node;
        public string portName;
        public bool isMulti;

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
    }
}
