
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using UnityEditor.UIElements;
using System.Reflection;

/// <summary>
/// Node elements that are added to the graph canvas.
/// These are the links between the visual rendering of a node 
/// and the underlying node data itself. 
/// </summary>
public class NodeView : Node
{
    EdgeConnectorListener m_ConnectorListener;
    
    public AbstractNode NodeData;

    List<NodePort> m_Inputs;
    List<NodePort> m_Outputs;
    
    public void Initialize(AbstractNode data, Vector2 position, EdgeConnectorListener connectorListener)
    {
        m_ConnectorListener = connectorListener;
        NodeData = data;
        
        // Set the underlying VisualElement's ViewDataKey.
        // This will allow us to use the GraphView's Guid lookup methods on nodes.
        viewDataKey = data.guid.ToString();

        // clone ports from the type?

        // TODO: I'm running into reference problems here. The lookup will return
        // a template of all the slots, but empty connection info. 
        // BUT, this can also be instantiated from something that DOES have connection info
        // and it's expected to be able to update that underlying data accordingly. 

        // Really, maybe it's smarter to split off connection info from the type field? 
        // But at the same time.. I want to persist all that so sHRUg.

        // Basically there's two ways as node view can be created:
        // From a node that was deserialized (NodeData loaded from some source)
        // or from a node that was created via the search.

        title = data.Name + " - " + data.guid;
        // elementTypeColor = TypeData.HeaderTheme;
        
        SetPosition(new Rect(position.x, position.y, 0, 0));
        
        AddPorts();
        AddControls();
        
        MarkDirtyRepaint();
    }

    internal void UpdateLinkedData()
    {
        // Refresh port connection information
        m_Inputs.ForEach((o) => o.UpdatePortData());
        m_Outputs.ForEach((o) => o.UpdatePortData());

        NodeData.GraphPosition = GetPosition().position;
        NodeData.RegenerateGuid();
    }
    
    /// <summary>
    /// Read port information from the linked data and add interactive elements
    /// </summary>
    private void AddPorts()
    {
        // Technically can just be the container + a cast. But, slow.
        m_Inputs = new List<NodePort>();
        m_Outputs = new List<NodePort>();
        
        var ports = NodeData.Ports;
        for (int i = 0; i < ports.Count; i++)
        {
            if (ports[i].PortType == PortType.Output)
            {
                var output = NodePort.Create(ports[i], m_ConnectorListener);

                m_Outputs.Add(output);
                outputContainer.Add(output);
            }
            else if (ports[i].PortType == PortType.Input)
            {
                var input = NodePort.Create(ports[i], m_ConnectorListener);

                m_Inputs.Add(input);
                inputContainer.Add(input);
            }
        }
    }
    
    private void AddControlRow(string label, VisualElement control)
    {
        VisualElement row = new VisualElement();
        row.AddToClassList("ExtensionRow");

        row.Add(new Label(label));
        row.Add(control);

        extensionContainer.Add(row);
        //extensionContainer.Add(row);
    }

    /// <summary>
    /// Add additional non-port elements (inputs, checkboxes, textures, etc)
    /// </summary>
    private void AddControls()
    {
        // .graphView gives us the GraphView
        // .extensionContainer gives us a zone to drop everything

       /* var serialized = new SerializedObject(m_NodeData);
        
        var prop = serialized.GetIterator();
        prop.NextVisible(true); // Expand first child
        do
        {
            var uieProp = new PropertyField(prop);
            extensionContainer.Add(uieProp);
        }
        while (prop.NextVisible(false));*/

        // Unfortunately the UIElement shortcuts above won't work here.
        // We can't make individual nodes be ScriptableObjects without
        // hellfire raining down and breaking everything.

        
        var fields = NodeData.GetType().GetFields(
            BindingFlags.Public | BindingFlags.Instance
        );
        
        foreach (var field in fields)
        {
            // Lazy exclusion rule
            if (field.GetCustomAttribute<InputAttribute>() == null &&
                field.GetCustomAttribute<OutputAttribute>() == null)
            {
                AddFieldInput(field);
            }
        }
        
        // Toggle visibility of the extension container
        RefreshExpandedState();
    }

    protected void AddFieldInput(FieldInfo fieldInfo)
    {
        object value = fieldInfo.GetValue(NodeData);
        
        if (value is Enum)
        {
            // Need to somehow pull the bind value of the node here.
            var field = new EnumField(value as Enum);
            field.RegisterValueChangedCallback((e) =>
            {
                fieldInfo.SetValue(NodeData, e.newValue);
                Debug.Log(e.newValue);
            });
            AddControlRow(fieldInfo.Name, field);
        }
        else if (value is bool)
        {
            var field = new Toggle();
            field.value = (bool)value;

            field.RegisterValueChangedCallback((e) =>
            {
                fieldInfo.SetValue(NodeData, e.newValue);
                Debug.Log(e.newValue);
            });
            AddControlRow(fieldInfo.Name, field);
        }
        else if (value is float)
        {
            var field = new FloatField();
            field.value = (float)value;

            field.RegisterValueChangedCallback((e) =>
            {
                fieldInfo.SetValue(NodeData, e.newValue);
                Debug.Log(e.newValue);
            });
            AddControlRow(fieldInfo.Name, field);
        }
        else if (value is Vector3)
        {
            var field = new Vector3Field();
            field.value = (Vector3)value;
                
            field.RegisterValueChangedCallback((e) =>
            {
                fieldInfo.SetValue(NodeData, e.newValue);
                Debug.Log(e.newValue);
            });
            AddControlRow(fieldInfo.Name, field);
        }
        else if (value is AnimationCurve)
        {
            Debug.Log("is animation curve");
            var field = new CurveField();
            field.value = (AnimationCurve)value;
                
            field.RegisterValueChangedCallback((e) =>
            {
                fieldInfo.SetValue(NodeData, e.newValue);
                Debug.Log(e.newValue);
            });
            AddControlRow(fieldInfo.Name, field);
        }
        else if (fieldInfo.FieldType == typeof(GameObject))
        {
            Debug.Log("Is object field");
            var field = new ObjectField();
            field.objectType = typeof(GameObject);
            field.value = value as UnityEngine.Object;
                
            field.RegisterValueChangedCallback((e) =>
            {
                fieldInfo.SetValue(NodeData, e.newValue);
                Debug.Log(e.newValue);
            });
            AddControlRow(fieldInfo.Name, field);
        }
    }
    
    public NodePort GetCompatibleInputPort(NodePort output)
    { 
        foreach (var port in m_Inputs)
        {
            if (port.IsCompatibleWith(output))
            {
                return port;
            }
        }

        return null;
    }
    
    public NodePort GetCompatibleOutputPort(NodePort input)
    { 
        foreach (var port in m_Outputs)
        {
            if (port.IsCompatibleWith(input))
            {
                return port;
            }
        }

        return null;
    }

    public NodePort GetInputPort(string name)
    {
        foreach (var port in m_Inputs)
        {
            if (port.portName == name)
            {
                return port;
            }
        }

        return null;
    }

    public NodePort GetOutputPort(string name)
    {
        foreach (var port in m_Outputs)
        {
            if (port.portName == name)
            {
                return port;
            }
        }

        return null;
    }
}
