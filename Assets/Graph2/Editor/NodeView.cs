using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Graph2
{
    public class NodeView : Node
    {
        public AbstractNode NodeData { get; protected set; }

        EdgeConnectorListener m_ConnectorListener;

        SerializedObject m_SerializedNode;

        // TODO: Don't really want this public but DestroyNode uses it.
        public Dictionary<string, PortView> InputPorts = new Dictionary<string, PortView>();
        public Dictionary<string, PortView> OutputPorts = new Dictionary<string, PortView>();

        public void Initialize(AbstractNode node, EdgeConnectorListener connectorListener)
        {
            NodeData = node;
            SetPosition(new Rect(node.Position.x, node.Position.y, 0, 0));
            m_ConnectorListener = connectorListener;
            title = node.name;

            m_SerializedNode = new SerializedObject(node);
            
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
            var fields = NodeData.GetType().GetFields(
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance
            );
            
            foreach (var field in fields)
            {
                var name = field.Name;

                foreach (var attr in field.GetCustomAttributes(true))
                {
                    Debug.Log(attr);

                    if (attr is InputAttribute)
                    {
                        var port = NodeData.GetInputPort(name);
                        if (port == null)
                        {
                            // Introspection pulled up a new port, track it.
                            port = new NodePort()
                            {
                                fieldName = name,
                                node = NodeData,
                                allowMany = true
                            };

                            NodeData.Inputs.Add(port);
                        }
                
                        Debug.Log("Add input: " + name);
                        AddInputPort(
                            port, 
                            m_SerializedNode.FindProperty(field.Name), 
                            field.FieldType
                        );
                    }
                    else if (attr is OutputAttribute)
                    {
                        var port = NodeData.GetOutputPort(name);
                        if (port == null)
                        {
                            port = new NodePort()
                            {
                                fieldName = name,
                                node = NodeData,
                                allowMany = true
                            };

                            NodeData.Outputs.Add(port);
                        }
                
                        Debug.Log("Add output: " + name);
                        AddOutputPort(port, null, field.FieldType);
                    }
                    else if (attr is EditableAttribute)
                    {
                        AddEditableField(m_SerializedNode.FindProperty(field.Name));
                    }
                }
                
            }
            
            // TODO: Deal with deleted/renamed ports.

            
            // Toggle visibility of the extension container
            RefreshExpandedState();
        }

        protected void AddEditableField(SerializedProperty prop)
        {
            var field = new PropertyField(prop);
            field.Bind(m_SerializedNode);
            field.RegisterCallback<ChangeEvent<string>>((evt) =>
            {
                Debug.Log(evt);
            });
            
            extensionContainer.Add(field);
        }

        protected void AddInputPort(NodePort port, SerializedProperty prop, Type type)
        {
            var view = PortView.Create(
                port, 
                Orientation.Horizontal,
                Direction.Input, 
                prop, 
                type,
                m_ConnectorListener
            );
            
            InputPorts.Add(port.fieldName, view);
            inputContainer.Add(view);
        }
        
        protected void AddOutputPort(NodePort port, SerializedProperty prop, Type type)
        {
            var view = PortView.Create(
                port, 
                Orientation.Horizontal, 
                Direction.Output, 
                prop,
                type, 
                m_ConnectorListener
            );

            OutputPorts.Add(port.fieldName, view);
            outputContainer.Add(view);
        }

        public PortView GetInputPort(string name)
        {
            InputPorts.TryGetValue(name, out PortView port);
            return port;
        }

        public PortView GetOutputPort(string name)
        {
            OutputPorts.TryGetValue(name, out PortView port);
            return port;
        }
        
        public PortView GetCompatibleInputPort(PortView output)
        { 
            return InputPorts.FirstOrDefault(
                (port) => port.Value.IsCompatibleWith(output)
            ).Value;
        }
    
        public PortView GetCompatibleOutputPort(PortView input)
        {
            return OutputPorts.FirstOrDefault(
                (port) => port.Value.IsCompatibleWith(input)
            ).Value;
        }
    }
}
