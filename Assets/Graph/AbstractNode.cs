using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A serializable node that can exist on the graph
/// </summary>
[Serializable]
public abstract class AbstractNode : ISerializationCallbackReceiver
{
    public Guid guid { get { return m_Guid; } }

    public string Name { get { return m_Name; } }

    [NonSerialized]
    public List<NodePortData> Ports = new List<NodePortData>();

    [NonSerialized]
    Guid m_Guid;

    [SerializeField]
    string m_Name;

    [SerializeField]
    NodePortData[] m_PortsSerialized;

    [SerializeField]
    public Vector2 GraphPosition;

    [SerializeField]
    string m_GuidSerialized;

    bool m_Dirty;

    public void InitializeFromType(NodeType type)
    {
        m_Name = type.Name;
        m_Guid = Guid.NewGuid();

        // Copy ports. TODO: .. not this.
        Ports = new List<NodePortData>();
        if (type.Ports != null)
        {
            foreach (var src in type.Ports)
            {
                Ports.Add(new NodePortData()
                {
                    Name = src.Name,
                    DisplayName = src.DisplayName,
                    Node = this,
                    PortType = src.PortType,
                    FieldType = src.FieldType,
                    ConnectedGuids = src.ConnectedGuids
                });
            }
        }
    }

    public void Dirty()
    {
        m_Dirty = true;
    }

    public void RegenerateGuid()
    {
        m_Guid = Guid.NewGuid();
    }

    public void OnBeforeSerialize()
    {
        // Same as SerializableGraph - this is called CONSTANTLY
        // via the inspector

        // Debug.Log("Before serialization");
        m_GuidSerialized = guid.ToString();
        m_PortsSerialized = Ports.ToArray();
    }

    public void OnAfterDeserialize()
    {
        Debug.Log("After serialization");
        if (!string.IsNullOrEmpty(m_GuidSerialized))
        {
            m_Guid = new Guid(m_GuidSerialized);
        }

        Ports = new List<NodePortData>(m_PortsSerialized.Length);
        foreach (var port in m_PortsSerialized)
        {
            port.Node = this;
            Ports.Add(port);
        }
    }

    public NodePortData GetInputPort(string name)
    {
        return Ports.Find(
            (port) => port.PortType == PortType.Input && port.Name == name
        );
    }

    public NodePortData GetOutputPort(string name)
    {
        return Ports.Find(
            (port) => port.PortType == PortType.Output && port.Name == name
        );
    }
}
