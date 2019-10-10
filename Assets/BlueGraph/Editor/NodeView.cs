
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using BlueGraph;

namespace BlueGraphEditor
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CustomNodeViewAttribute : Attribute
    {
        public Type nodeType;

        public CustomNodeViewAttribute(Type nodeType)
        {
            this.nodeType = nodeType; 
        }
    }

    public class NodeView : Node, ICanDirty
    {
        public AbstractNode target;
        
        public List<PortView> inputs = new List<PortView>();
        public List<PortView> outputs = new List<PortView>();
        
        EdgeConnectorListener m_ConnectorListener;
        SerializedObject m_SerializedNode;

        public virtual void Initialize(AbstractNode node, EdgeConnectorListener connectorListener)
        {
            viewDataKey = node.guid;
            
            // TODO: Less hardcoded of a path
            StyleSheet styles = AssetDatabase.LoadAssetAtPath<StyleSheet>(
                "Assets/BlueGraph/Editor/Styles/NodeView.uss"
            );
        
            styleSheets.Add(styles);
            AddToClassList("node-view");
            
            target = node;
            SetPosition(new Rect(node.position.x, node.position.y, 0, 0));
            m_ConnectorListener = connectorListener;
            title = node.name;
            
            m_SerializedNode = new SerializedObject(node);

            // Custom OnDestroy() handler via https://forum.unity.com/threads/request-for-visualelement-ondestroy-or-onremoved-event.718814/
            RegisterCallback<DetachFromPanelEvent>((e) => OnDestroy());
            
            UpdatePorts();
        }

        /// <summary>
        /// Executed when we're about to detach this element from the graph. 
        /// </summary>
        protected virtual void OnDestroy()
        {
            
        }
        
        /// <summary>
        /// Make sure our list of PortViews sync up with our NodePorts
        /// </summary>
        public void UpdatePorts()
        {
            var reflectionData = NodeReflection.GetNodeType(target.GetType());

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
            var port = target.GetInputPort(portData.portName);
            if (port == null)
            {
                port = new NodePort()
                {
                    node = target,
                    portName = portData.portName,
                    isMulti = portData.isMulti
                };

                target.inputs.Add(port);
            }
            
            var view = PortView.Create(
                port, 
                portData,
                m_SerializedNode.FindProperty(portData.fieldName), 
                portData.type,
                m_ConnectorListener
            );
            
            inputs.Add(view);
            inputContainer.Add(view);
        }
        
        protected void AddOutputPort(PortReflectionData portData)
        {
            var port = target.GetOutputPort(portData.portName);
            if (port == null)
            {
                // Introspection pulled up a new port, track it.
                port = new NodePort()
                {
                    node = target,
                    portName = portData.portName,
                    isMulti = portData.isMulti
                };

                target.outputs.Add(port);
            }

            var view = PortView.Create(
                port, 
                portData,
                m_SerializedNode.FindProperty(portData.fieldName), 
                portData.type,
                m_ConnectorListener
            );
            
            outputs.Add(view);
            outputContainer.Add(view);
        }

        public PortView GetInputPort(string name)
        {
            return inputs.Find((port) => port.portName == name);
        }

        public PortView GetOutputPort(string name)
        {
            return outputs.Find((port) => port.portName == name);
        }
        
        public PortView GetCompatibleInputPort(PortView output)
        { 
            return inputs.Find((port) => port.IsCompatibleWith(output));
        }
    
        public PortView GetCompatibleOutputPort(PortView input)
        {
            return outputs.Find((port) => port.IsCompatibleWith(input));
        }

        /// <summary>
        /// Dirty this node in response to a change in connectivity. Invalidate
        /// any cache in prep for an OnUpdate() followup call. 
        /// </summary>
        public virtual void OnDirty()
        {
            // Dirty all ports so they can refresh their state
            foreach (var port in inputs)
            {
                port.OnDirty();
            }

            foreach (var port in outputs)
            {
                port.OnDirty();
            }
        }

        /// <summary>
        /// Called when this node was dirtied and the UI is redrawing. 
        /// </summary>
        public virtual void OnUpdate()
        {
            // Propagate update to all ports
            foreach (var port in inputs)
            {
                port.OnUpdate();
            }

            foreach (var port in outputs)
            {
                port.OnUpdate();
            }
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            target.position = newPos.position;
        }
    }
}
