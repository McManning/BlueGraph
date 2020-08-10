
using System;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using GraphViewPort = UnityEditor.Experimental.GraphView.Port;
using System.Collections;

namespace BlueGraph.Editor
{
    public class PortView : GraphViewPort
    {
        public Port target;

        /// <summary>
        /// Should the inline editor field disappear once one or more
        /// connections have been made to this port view
        /// </summary>
        public bool hideEditorFieldOnConnection = true;

        VisualElement m_EditorField;
        
        public PortView(
            Orientation portOrientation, 
            Direction portDirection, 
            Capacity portCapacity, 
            Type type
        ) : base(portOrientation, portDirection, portCapacity, type)
        {
            styleSheets.Add(Resources.Load<StyleSheet>("Styles/PortView"));
            AddToClassList("portView");
            AddTypeClasses(type);

            visualClass = "";
            //visualClass = GetTypeVisualClass(type);
            tooltip = type.ToPrettyName();
        }
    
        public static PortView Create(Port port, IEdgeConnectorListener connectorListener) 
        {
            Direction direction = port.direction == PortDirection.Input ? Direction.Input : Direction.Output;
            Capacity capacity = port.capacity == PortCapacity.Multiple ? Capacity.Multi : Capacity.Single;

            var view = new PortView(Orientation.Horizontal, direction, capacity, port.type) 
            {
                m_EdgeConnector = new EdgeConnector<Edge>(connectorListener),
                portName = port.name,
                target = port
            };

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
        /// Add USS class names for the given type
        /// </summary>
        void AddTypeClasses(Type type)
        {
            var classes = type.ToUSSClasses();
            foreach (var cls in classes) {
                AddToClassList(cls);
            }
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
