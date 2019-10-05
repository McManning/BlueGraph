
using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using System.Collections.Generic;
using UnityEditor.UIElements;

namespace Graph2
{
    public class PortView : Port
    {
        protected PortView(Orientation portOrientation, Direction portDirection, Capacity portCapacity, Type type) 
            : base(portOrientation, portDirection, portCapacity, type)
        {
            
        }
    
        public static PortView Create(NodePort port, Orientation portOrientation, Direction portDirection, Type type, IEdgeConnectorListener connectorListener)
        {
            var view = new PortView(
                portOrientation, 
                portDirection, 
                port.allowMany ? Capacity.Multi : Capacity.Single, 
                null
            ) {
                m_EdgeConnector = new EdgeConnector<Edge>(connectorListener),
                portName = port.fieldName,
                userData = port,
                visualClass = type.FullName // TODO: Cleanup.
            };
        
            view.AddManipulator(view.m_EdgeConnector);
            return view;
        }

        /// <summary>
        /// Return true if this port can be connected with an edge to the given port
        /// </summary>
        public bool IsCompatibleWith(PortView other)
        {
            // Note: direction should be account for here as well. And possibly
            // any type of loop detection to ensure nobody is making a cycle 
            // (for certain use cases, that is)

            // For now, just make it exact based on type classification
            return visualClass == other.visualClass;
        }
    }
}
