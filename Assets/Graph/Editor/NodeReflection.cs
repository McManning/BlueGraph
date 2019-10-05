
using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

/// <summary>
/// Class reflection tooling for graph nodes. Used in aggregating available nodes
/// in the SearchProvider and generating NodeType data from reflection and attributes
/// </summary>
public static class NodeReflection
{
    private static Dictionary<Type, NodeType> k_NodeTypes = null;
    private static Dictionary<Type, NodePortData[]> k_NodePorts = null;

    /// <summary>
    /// Extract node type info for one node
    /// </summary>
    public static NodeType GetNodeType(Type t)
    {
        var types = GetNodeTypes();

        if (types.ContainsKey(t))
        {
            return types[t];
        }

        return null;
    }

    /// <summary>
    /// Get all types derived from the base node
    /// </summary>
    /// <returns></returns>
    public static Dictionary<Type, NodeType> GetNodeTypes()
    {
        if (k_NodeTypes != null)
        {
            return k_NodeTypes;
        }

        var baseType = typeof(AbstractNode);
        var types = new List<Type>();
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (var assembly in assemblies)
        {
            try
            {
                types.AddRange(assembly.GetTypes().Where(
                    (t) => !t.IsAbstract && baseType.IsAssignableFrom(t)).ToArray()
                );
            } 
            catch (ReflectionTypeLoadException) { }
        }
        
        var nodeTypes = new Dictionary<Type, NodeType>();
        foreach (var type in types) 
        {
            var attr = type.GetCustomAttribute<GraphNodeAttribute>();
            if (attr != null)
            {
                nodeTypes[type] = new NodeType()
                {
                    Name = attr.Name,
                    HeaderTheme = attr.HeaderTheme,
                    Ports = GetNodePorts(type),
                    InstanceType = type
                };
            }
        }
        
        k_NodeTypes = nodeTypes;
        return k_NodeTypes;
    }

    /// <summary>
    /// Extract NodeField information from class reflection + attributes
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static NodePortData[] GetNodePorts(Type type)
    {
        if (k_NodePorts == null)
        {
            k_NodePorts = new Dictionary<Type, NodePortData[]>();
        }
        
        if (k_NodePorts.ContainsKey(type))
        {
            return k_NodePorts[type];
        }

        var fields = new List<FieldInfo>(type.GetFields(
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
        ));
        
        // Iterate through inherited private fields as well
        var temp = type;
        while ((temp = temp.BaseType) != typeof(AbstractNode))
        {
            fields.AddRange(temp.GetFields(BindingFlags.NonPublic | BindingFlags.Instance));
        }
        
        // Extract port metadata from each tagged field
        var ports = new List<NodePortData>();
        for (int i = 0; i < fields.Count; i++)
        {
            NodePortData port = new NodePortData()
            {
                Name = fields[i].Name,
                DisplayName = ObjectNames.NicifyVariableName(fields[i].Name),
                FieldType = fields[i].FieldType
            };

            bool isValidNode = false;

            object[] attribs = fields[i].GetCustomAttributes(true);
            for (int j = 0; j < attribs.Length; j++)
            {
                if (attribs[j] is InputAttribute)
                {
                    var attr = attribs[j] as InputAttribute;
                    if (attr.Name != null) port.DisplayName = attr.Name;

                    port.InputConstraint = attr.InputConstraint;
                    port.PortType = PortType.Input;

                    isValidNode = true;
                }
                else if (attribs[j] is OutputAttribute)
                {
                    var attr = attribs[j] as OutputAttribute;
                    if (attr.Name != null) port.DisplayName = attr.Name;

                    port.PortType = PortType.Output;

                    isValidNode = true;
                }
            }

            // also add everything else as a constant port, for testing.
            if (!isValidNode)
            {
                port.PortType = PortType.Constant;
                isValidNode = true;
            }

            if (isValidNode)
            {
                ports.Add(port);
            }
        }

        var portsArr = ports.ToArray();
        k_NodePorts[type] = portsArr;
        return portsArr;
    }
}
