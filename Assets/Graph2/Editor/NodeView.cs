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
            // TODO: Less hardcoded of a path
            StyleSheet styles = AssetDatabase.LoadAssetAtPath<StyleSheet>(
                "Assets/Graph/Editor/Styles/NodeView.uss"
            );
        
            styleSheets.Add(styles);
            
            NodeData = node;
            SetPosition(new Rect(node.position.x, node.position.y, 0, 0));
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
            var reflectionData = NodeReflection.GetNodeType(NodeData.GetType());

            foreach (var portData in reflectionData.ports)
            {
                if (portData.isInput)
                {
                    AddInputPort(portData);
                }
                else
                {
                    AddOutputPort(portData);
                }
            }

            foreach (var editable in reflectionData.editables)
            {
                AddEditableField(m_SerializedNode.FindProperty(editable.fieldName));
            }
            
            // TODO: Deal with deleted/renamed ports.
            
            // Toggle visibility of the extension container
            RefreshExpandedState();
        }

        protected void AddEditableField(SerializedProperty prop)
        {
            var field = new PropertyField(prop);
            field.Bind(m_SerializedNode);
            
            extensionContainer.Add(field);
        }

        protected void AddInputPort(PortReflectionData portData)
        {
            var port = NodeData.GetInputPort(portData.fieldName);
            if (port == null)
            {
                port = new NodePort()
                {
                    node = NodeData,
                    fieldName = portData.fieldName,
                    isMulti = portData.isMulti
                };

                NodeData.Inputs.Add(port);
            }

            var view = PortView.Create(
                port, 
                portData,
                m_SerializedNode.FindProperty(port.fieldName), 
                portData.type,
                m_ConnectorListener
            );
            
            InputPorts.Add(port.fieldName, view);
            inputContainer.Add(view);
        }
        
        protected void AddOutputPort(PortReflectionData portData)
        {
            var port = NodeData.GetOutputPort(name);
            if (port == null)
            {
                // Introspection pulled up a new port, track it.
                port = new NodePort()
                {
                    node = NodeData,
                    fieldName = portData.fieldName,
                    isMulti = portData.isMulti
                };

                NodeData.Outputs.Add(port);
            }

            var view = PortView.Create(
                port, 
                portData,
                m_SerializedNode.FindProperty(port.fieldName), 
                portData.type,
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

        /// <summary>
        /// Dirty this node in response to a change in connectivity. Invalidate
        /// any cache in prep for an OnUpdate() followup call. 
        /// </summary>
        public void OnDirty()
        {
            // Dirty all ports so they can refresh their state
            foreach (var port in InputPorts.Values)
            {
                port.OnDirty();
            }

            foreach (var port in OutputPorts.Values)
            {
                port.OnDirty();
            }
        }

        /// <summary>
        /// Called when this node was dirtied and the UI is redrawing. 
        /// </summary>
        public void OnUpdate()
        {
            // Propagate to all ports
            foreach (var port in InputPorts.Values)
            {
                port.OnUpdate();
            }

            foreach (var port in OutputPorts.Values)
            {
                port.OnUpdate();
            }
        }
    }
}
