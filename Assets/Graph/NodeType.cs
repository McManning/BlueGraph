using System;
using UnityEngine;

/// <summary>
/// Type information for a node decoded from reflection + attributes
/// </summary>
[Serializable]
public class NodeType : ISerializationCallbackReceiver
{
    public string Name;
    public Color HeaderTheme;
    
    // TODO: Awkward that port data contains connection info as well. 
    // But we need to pass it in from the search provider when we 
    // perform reflection on all available nodes. Can't generate the
    // port list from within NodeData because it shouldn't be
    // exposed to editor tools.
    [NonSerialized]
    public NodePortData[] Ports;

    // Underlying class type to instantiate
    public Type InstanceType;
    
    [SerializeField]
    string m_TypeSerialized;
    
    public void OnBeforeSerialize()
    {
        if (InstanceType != null)
        {
            m_TypeSerialized = InstanceType.Name;
        }
    }

    public void OnAfterDeserialize()
    {
        if (!string.IsNullOrEmpty(m_TypeSerialized))
        {
            // TODO: This is probably trash. Assemblies, etc.
            InstanceType = Type.GetType(m_TypeSerialized);
        }
    }
}
