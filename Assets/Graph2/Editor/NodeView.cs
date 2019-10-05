using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Graph2
{
    public class NodeView : Node
    {
        public AbstractNode NodeData { get; protected set; }

        EdgeConnectorListener m_ConnectorListener;

        Dictionary<string, PortView> m_InputPortViews = new Dictionary<string, PortView>();
        Dictionary<string, PortView> m_OutputPortViews = new Dictionary<string, PortView>();

        public void Initialize(AbstractNode node, EdgeConnectorListener connectorListener)
        {
            NodeData = node;
            SetPosition(new Rect(node.Position.x, node.Position.y, 0, 0));
            m_ConnectorListener = connectorListener;
            title = node.name;

            UpdatePorts();
        }

        /// <summary>
        /// Make sure our list of PortViews sync up with our NodePorts
        /// </summary>
        public void UpdatePorts()
        {
            // Extract fields from the node.
            // TODO: Eventually, this'll cache somewhere per node type. 
            // Also attribute reads.
            var fields = NodeData.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            
            NodePort port;
            foreach (var field in fields)
            {
                var name = field.Name;
                port = NodeData.GetInputPort(name);
                
                // Introspection pulled up a new port, track it.
                if (port == null)
                {
                    port = new NodePort()
                    {
                        fieldName = name,
                        node = NodeData,
                        allowMany = true
                    };

                    NodeData.Inputs.Add(port);
                }
                
                AddInputPort(port, field.FieldType);
            }
            
            // TODO: Deal with deleted/renamed ports.


            // Make a fake output.

            port = NodeData.GetOutputPort("Out");
            if (port == null)
            {
                port = new NodePort()
                {
                    fieldName = "Out",
                    node = NodeData
                };

                NodeData.Outputs.Add(port);
            }

            AddOutputPort(port, typeof(float));
        }

        protected void AddInputPort(NodePort port, Type type)
        {
            var view = PortView.Create(
                port, 
                Orientation.Horizontal,
                Direction.Input, 
                type, 
                m_ConnectorListener
            );

            m_InputPortViews.Add(port.fieldName, view);
            inputContainer.Add(view);
        }
        
        protected void AddOutputPort(NodePort port, Type type)
        {
            var view = PortView.Create(
                port, 
                Orientation.Horizontal, 
                Direction.Output, 
                type, 
                m_ConnectorListener
            );

            m_OutputPortViews.Add(port.fieldName, view);
            outputContainer.Add(view);
        }

        public PortView GetInputPort(string name)
        {
            m_InputPortViews.TryGetValue(name, out PortView port);
            return port;
        }

        public PortView GetOutputPort(string name)
        {
            m_OutputPortViews.TryGetValue(name, out PortView port);
            return port;
        }
        
        public PortView GetCompatibleInputPort(PortView output)
        { 
            return m_InputPortViews.FirstOrDefault(
                (port) => port.Value.IsCompatibleWith(output)
            ).Value;
        }
    
        public PortView GetCompatibleOutputPort(PortView input)
        {
            return m_OutputPortViews.FirstOrDefault(
                (port) => port.Value.IsCompatibleWith(input)
            ).Value;
        }
    }
}
