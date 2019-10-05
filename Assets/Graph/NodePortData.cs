using System;
using System.Collections.Generic;
using UnityEngine;

public enum PortType
{
    Constant = 0,
    Input = 1,
    Output = 2
}

public class NodePortConnection
{
    public AbstractNode Node;
    public string PortName;
}

/// <summary>
/// Serializable metadata about a node port
/// </summary>
[Serializable]
public class NodePortData : ISerializationCallbackReceiver
{
    /// <summary>
    /// Parent node
    /// </summary>
    public AbstractNode Node;

    // TODO: Needed anymore? I'm using port names now for links
    public Guid guid => m_Guid;

    public string Name;

    public string DisplayName;
    
    /// <summary>
    /// Constraint applied to acceptable input data types
    /// </summary>
    public NodeInputConstraint InputConstraint;
    
    /// <summary>
    /// Unique ID of the node ports this port is linked to
    /// </summary>
    public string[] ConnectedGuids; 

    [NonSerialized]
    public List<NodePortConnection> Connections = new List<NodePortConnection>();

    [NonSerialized]
    Guid m_Guid;
    
    [SerializeField]
    string m_GuidSerialized;

    public PortType PortType;
    
    [NonSerialized]
    public Type FieldType;

    [SerializeField]
    string m_FieldType;
    
    public NodePortData()
    {
        m_Guid = Guid.NewGuid();
        InputConstraint = NodeInputConstraint.Any;
    }
    
    public string GetVisualClass()
    {
        if (FieldType != null)
        {
            return FieldType.FullName;
        }

        // No type info available
        return null;
    }

    public void OnBeforeSerialize()
    {
        m_GuidSerialized = guid.ToString();
        m_FieldType = FieldType.FullName;
    }

    public void OnAfterDeserialize()
    {
        if (!string.IsNullOrEmpty(m_GuidSerialized))
        {
            m_Guid = new Guid(m_GuidSerialized);
        }

        FieldType = Type.GetType(m_FieldType);
    }
}
