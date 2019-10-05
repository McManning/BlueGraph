
using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using System.Collections.Generic;
using UnityEditor.UIElements;
using System.Reflection;

/*
/// <summary>
/// Edges added to the graph
/// </summary>
public class EdgeView : Edge
{
    // TECHNICALLY I don't need this. Can just use native Edges. 
    // But if I wanted to do some custom dumb rendering thing I'd use it.
    // Or adding some metadata per edge
    
    public void Initialize(Port portIn, Port portOut)
    {
        input = portIn;
        output = portOut;
        
        input.Connect(this);
        output.Connect(this);
    }
}
*/

// Based on: https://github.com/Unity-Technologies/ScriptableRenderPipeline/blob/dc09aba6a4cbd997f11e32a51881bf91d1b55b5e/com.unity.shadergraph/Editor/Drawing/Views/ShaderPort.cs
public class NodePort : Port
{
    /// <summary>
    /// Serializable data associated with this port
    /// </summary>
    public NodePortData PortData;

    protected NodePort(Orientation portOrientation, Direction portDirection, Capacity portCapacity, Type type) 
        : base(portOrientation, portDirection, portCapacity, type)
    {
        // TODO: Load node CSS? Or, alternatively, just keep it at the top level graph CSS.
    }
    
    public static NodePort Create(NodePortData data, IEdgeConnectorListener connectorListener)
    {
        var port = new NodePort(
            Orientation.Horizontal,
            data.PortType == PortType.Output ? Direction.Output : Direction.Input,
            Capacity.Single,
            null
        ) {
            m_EdgeConnector = new EdgeConnector<Edge>(connectorListener),
            PortData = data,
            portName = data.Name,
            visualClass = data.GetVisualClass()
        };
        
        if (data.PortType == PortType.Input)
        {
            var fieldInfo = data.Node.GetType().GetField(
                data.Name, 
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
            );

            object value = fieldInfo.GetValue(data.Node);

            Debug.Log(fieldInfo.GetType());

            if (value is Enum)
            {
                // Need to somehow pull the bind value of the node here.
                var field = new EnumField(value as Enum);
                field.RegisterValueChangedCallback((e) =>
                {
                    fieldInfo.SetValue(data.Node, e.newValue);
                    Debug.Log(e.newValue);
                });
                port.m_ConnectorBox.parent.Add(field);
            }
            else if (value is float)
            {
                var field = new FloatField();
                field.value = (float)value;

                field.RegisterValueChangedCallback((e) =>
                {
                    fieldInfo.SetValue(data.Node, e.newValue);
                    Debug.Log(e.newValue);
                });
                port.m_ConnectorBox.parent.Add(field);
            }
            else if (value is Vector3)
            {
                var field = new Vector3Field();
                field.value = (Vector3)value;
                
                field.RegisterValueChangedCallback((e) =>
                {
                    fieldInfo.SetValue(data.Node, e.newValue);
                    Debug.Log(e.newValue);
                });
                port.m_ConnectorBox.parent.Add(field);
            }
            else if (value is AnimationCurve)
            {
                Debug.Log("is animation curve");
                var field = new CurveField();
                field.value = (AnimationCurve)value;
                
                field.RegisterValueChangedCallback((e) =>
                {
                    fieldInfo.SetValue(data.Node, e.newValue);
                    Debug.Log(e.newValue);
                });
                port.m_ConnectorBox.parent.Add(field);
            }
            else if (fieldInfo.FieldType == typeof(GameObject))
            {
                Debug.Log("Is object field");
                var field = new ObjectField();
                field.objectType = typeof(GameObject);
                field.value = value as UnityEngine.Object;
                
                field.RegisterValueChangedCallback((e) =>
                {
                    fieldInfo.SetValue(data.Node, e.newValue);
                    Debug.Log(e.newValue);
                });
                port.m_ConnectorBox.parent.Add(field);
            }
        }

        port.AddManipulator(port.m_EdgeConnector);
        return port;
    }

    /// <summary>
    /// Return true if this port can be connected with an edge to the given port
    /// </summary>
    public bool IsCompatibleWith(NodePort other)
    {
        // Note: direction should be account for here as well. And possibly
        // any type of loop detection to ensure nobody is making a cycle 
        // (for certain use cases, that is)

        // For now, just make it exact based on type classification
        return (other.PortData.GetVisualClass() == PortData.GetVisualClass());
    }
    
    /// <summary>
    /// Update the linked data with connection information from the graph
    /// </summary>
    public void UpdatePortData()
    {
        var ids = new List<string>();
        
        PortData.Connections.Clear();

        foreach (var edge in connections)
        {
            var output = edge.output as NodePort;
            var input = edge.input as NodePort;
            
            // TODO: This might serialize both sides @ once
            if (output != this) 
            {
                PortData.Connections.Add(new NodePortConnection() {
                    Node = output.PortData.Node,
                    PortName = output.portName
                });
            }
            else if (input != this)
            {
                PortData.Connections.Add(new NodePortConnection() {
                    Node = input.PortData.Node,
                    PortName = input.portName
                });
            }
        }
    }
}
