
using System;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace BlueGraph.Editor
{
    public class PortView : UnityEditor.Experimental.GraphView.Port
    {
        public Port target;

        /// <summary>
        /// Should the inline editor field disappear once one or more
        /// connections have been made to this port view
        /// </summary>
        public bool hideEditorFieldOnConnection = true;

        VisualElement m_EditorField;
        
        protected PortView(
            Orientation portOrientation, 
            Direction portDirection, 
            Capacity portCapacity, 
            Type type
        ) : base(portOrientation, portDirection, portCapacity, type)
        {
            styleSheets.Add(Resources.Load<StyleSheet>("Styles/PortView"));
            AddToClassList("portView");
            
            visualClass = GetTypeVisualClass(type);
            tooltip = type.FullName;
        }
    
        public static PortView Create(
            Port port,
            Type type,
            IEdgeConnectorListener connectorListener
        ) {
            var view = new PortView(
                Orientation.Horizontal, 
                port.isInput ? Direction.Input : Direction.Output, 
                port.acceptsMultipleConnections ? Capacity.Multi : Capacity.Single, 
                type
            ) {
                m_EdgeConnector = new EdgeConnector<Edge>(connectorListener),
                portName = port.name,
                target = port
            };

            // Override default connector text with the human-readable port name
            // TODO: Apparently the edge connector (Edge.output.portName) is based on whatever
            // is in this label. So re-labeling it will inadvertedly change the port name. 
            // (or it might be a two way binding). So natively, we won't be able to have multiple
            // ports with the same name. 
            // view.m_ConnectorText.text = refPort.displayName;

            view.AddManipulator(view.m_EdgeConnector);
            return view;
        }

        public void SetEditorField(VisualElement field)
        {
            if (m_EditorField != null)
            {
                m_ConnectorBox.parent.Remove(m_EditorField);
            }

            m_EditorField = field;
            m_ConnectorBox.parent.Add(m_EditorField);
        }
        
        /// <summary>
        /// Return true if this port can be connected with an edge to the given port
        /// </summary>
        public bool IsCompatibleWith(PortView other)
        {
            if (other.node == node || other.direction == direction)
            {
                return false;
            }
            
            // TODO: Loop detection to ensure nobody is making a cycle 
            // (for certain use cases, that is)
            
            // Check for type cast support in the direction of output port -> input port
            return (other.direction == Direction.Input && portType.IsCastableTo(other.portType, true)) ||
                    (other.direction == Direction.Output && other.portType.IsCastableTo(portType, true));
        }
        
        /// <summary>
        /// Convert a Type to a USS class name
        /// </summary>
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
            if (connected && m_EditorField != null && hideEditorFieldOnConnection)
            {
                m_EditorField.style.display = DisplayStyle.None;
            }

            if (!connected && m_EditorField != null)
            {
                m_EditorField.style.display = DisplayStyle.Flex;
            }
        }
    }
}
