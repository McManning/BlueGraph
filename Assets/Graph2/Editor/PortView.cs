
using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEditor;

namespace Graph2
{
    public class PortView : Port
    {
        public NodePort target;

        VisualElement m_PropertyField;

        protected PortView(
            Orientation portOrientation, 
            Direction portDirection, 
            Capacity portCapacity, 
            Type type
        ) : base(portOrientation, portDirection, portCapacity, type)
        {
            // TODO: Less hardcoded of a path
            StyleSheet styles = AssetDatabase.LoadAssetAtPath<StyleSheet>(
                "Assets/Graph/Editor/Styles/PortView.uss"
            );
        
            styleSheets.Add(styles);
            
            visualClass = GetTypeVisualClass(type);
        }
    
        public static PortView Create(
            NodePort port, 
            PortReflectionData refPort,
            SerializedProperty prop, 
            Type type,
            IEdgeConnectorListener connectorListener
        ) {
            var view = new PortView(
                Orientation.Horizontal, 
                refPort.isInput ? Direction.Input : Direction.Output, 
                port.isMulti ? Capacity.Multi : Capacity.Single, 
                type
            ) {
                m_EdgeConnector = new EdgeConnector<Edge>(connectorListener),
                portName = port.portName,
                target = port
            };

            // Override default connector text with the human-readable port name
            // TODO: Apparently the edge connector (Edge.output.portName) is based on whatever
            // is in this label. So re-labeling it will inadvertedly change the port name. 
            // (or it might be a two way binding). So natively, we won't be able to have multiple
            // ports with the same name. 
            // view.m_ConnectorText.text = refPort.displayName;

            view.AddManipulator(view.m_EdgeConnector);

            // Bind to the underlying field
            if (prop != null && refPort.isEditable)
            {
                view.m_PropertyField = new PropertyField(prop, " ");
                view.m_PropertyField.Bind(prop.serializedObject);
                view.m_ConnectorBox.parent.Add(view.m_PropertyField);
            }

            return view;
        }

        /// <summary>
        /// Return true if this port can be connected with an edge to the given port
        /// </summary>
        public bool IsCompatibleWith(PortView other)
        {
            // Note: Loop detection to ensure nobody is making a cycle 
            // (for certain use cases, that is)
            
            return other.node != node
                && other.direction != direction
                && visualClass == other.visualClass;
        }

        public override void Disconnect(Edge edge)
        {
            // Copy the disconnect onto the linked data
            if (direction == Direction.Input)
            {
                target.Disconnect((edge.output.node as NodeView).NodeData, edge.output.portName);
            }
            else
            {
                target.Disconnect((edge.input.node as NodeView).NodeData, edge.input.portName);
            }
            
            base.Disconnect(edge);
        }

        public override void Connect(Edge edge)
        {
            // Copy the connect onto the linked data
            // TODO: This happens also when we load the graph for every node.
            // How can this be optimized out?
            if (direction == Direction.Input)
            {
                target.Connect((edge.output.node as NodeView).NodeData, edge.output.portName);
            }
            else
            {
                target.Connect((edge.input.node as NodeView).NodeData, edge.input.portName);
            }

            base.Connect(edge);
        }
        
        public string GetTypeVisualClass(Type type)
        {
            // TODO: Better variant that handles lists and such.

            if (type.IsEnum)
            {
                return "type-System-Enum";
            }

            return "type-" + type.FullName.Replace(".", "-");
        }

        /// <summary>
        /// Executed on change of a port connection. Perform any prep before the following
        /// OnUpdate() call during redraw. 
        /// </summary>
        public void OnDirty()
        {
            
        }
        
        /// <summary>
        /// Toggle visibility of the inline editable value based on whether we have connections
        /// </summary>
        public void OnUpdate()
        {
            if (connected && m_PropertyField != null)
            {
                m_PropertyField.visible = false;
            }

            if (!connected && m_PropertyField != null)
            {
                m_PropertyField.visible = true;
            }
        }
    }
}
