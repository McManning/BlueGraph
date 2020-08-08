using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

/// <summary>
/// Suite of reflection methods and caching for retrieving available
/// graph nodes and their associated editor views
/// </summary>
namespace BlueGraph.Editor
{
    /// <summary>
    /// Reflection data for a field with an [Input] or [Output] attribute
    /// </summary>
    public class PortReflectionData
    {
        public Type ConnectionType => field.FieldType;

        /// <summary>
        /// Associated class field if generated via Input/Output attributes
        /// </summary>
        public FieldInfo field;

        /// <summary>
        /// Display name for this port
        /// </summary>
        public string name;

        public bool isMulti;
        public bool isInput;
        public bool isEditable; // TODO: Rename?
        
        /// <summary>
        /// Create a VisualElement for this port's inline editor based on the field data type.
        /// 
        /// This returns null if the port is not marked as <c>isEditable</c> or the type
        /// could not be resolved to a supported control element.
        /// </summary>
        public VisualElement GetControlElement(NodeView view)
        {
            if (!isEditable)
            {
                return null;
            }

            return ControlElementFactory.CreateControl(field, view);
        }
    }

    /// <summary>
    /// Reflection data for a field with an [Editable] attribute.
    /// </summary>
    public class EditableReflectionData
    {
        public string name;
        public FieldInfo field;
        
        /// <summary>
        /// Create a VisualElement for this editable's inline editor based on the field data type.
        /// 
        /// This returns null if the type could not be resolved to a supported control element.
        /// </summary>
        public VisualElement GetControlElement(NodeView view)
        {
            return ControlElementFactory.CreateControl(field, view);
        }
    }

    /// <summary>
    /// Reflection data for a class with a [Node] attribute
    /// </summary>
    public class NodeReflectionData
    {
        /// <summary>
        /// Class type to instantiate for the node 
        /// </summary>
        public Type type;

        /// <summary>
        /// Module path for grouping nodes together
        /// </summary>
        public string[] path;

        /// <summary>
        /// Human-readable display name of the node. Will come from the last
        /// part of the path parsed out of node information - or be the class name.
        /// </summary>
        public string name;

        /// <summary>
        /// Content for node usage instructions
        /// </summary>
        public string help;

        public List<PortReflectionData> ports = new List<PortReflectionData>();
        public List<EditableReflectionData> editables = new List<EditableReflectionData>();
    
        /// <summary>
        /// Cache of FieldInfo entries on the node class
        /// </summary>
        public List<FieldInfo> fields = new List<FieldInfo>();

        public bool HasInputOfType(Type type)
        {
            foreach (var port in ports)
            {
                if (!port.isInput) continue;
       
                // Cast direction type -> port input
                if (type.IsCastableTo(port.ConnectionType, true))
                {
                    return true;
                }
            }

            return false;
        }

        public bool HasOutputOfType(Type type)
        {
            foreach (var port in ports)
            {
                if (port.isInput) continue;
       
                // Cast direction port output -> type
                if (port.ConnectionType.IsCastableTo(type, true))
                {
                    return true;
                }
            }

            return false;
        }
        
        /// <summary>
        /// Add ports based on attributes on the class fields.
        /// 
        /// This iterates through fields of a class and adds ports, editables, etc
        /// based on the attributes attached to each field. 
        /// </summary>
        public void AddFieldsFromClass(Type type)
        {
            fields.AddRange(type.GetFields(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
            ));
        
            // Extract port and editable metadata from each tagged field
            for (int i = 0; i < fields.Count; i++)
            {
                var attribs = fields[i].GetCustomAttributes(true);
                for (int j = 0; j < attribs.Length; j++)
                {
                    if (attribs[j] is InputAttribute input)
                    {
                        ports.Add(new PortReflectionData()
                        {
                            name = input.name ?? ObjectNames.NicifyVariableName(fields[i].Name),
                            field = fields[i],
                            isInput = true,
                            isMulti = input.multiple,
                            isEditable = input.editable
                        });
                    }
                    else if (attribs[j] is OutputAttribute output)
                    {
                        ports.Add(new PortReflectionData()
                        {
                            name = output.name ?? ObjectNames.NicifyVariableName(fields[i].Name),
                            field = fields[i],
                            isInput = false,
                            isMulti = output.multiple,
                            isEditable = false
                        });
                    }
                    else if (attribs[j] is EditableAttribute editable)
                    {
                        editables.Add(new EditableReflectionData()
                        {
                            name = editable.name ?? ObjectNames.NicifyVariableName(fields[i].Name),
                            field = fields[i]
                        });
                    }
                }
            }
        }

        /// <summary>
        /// Create a node instance from the reflected type data
        /// </summary>
        public AbstractNode CreateInstance()
        {
            var node = Activator.CreateInstance(type) as AbstractNode;
            node.name = name;
            
            // Create runtime ports from reflection data
            foreach (var port in ports)
            {
                node.AddPort(new Port {
                    ConnectionType = port.ConnectionType,
                    node = node,
                    name = port.name,
                    isMulti = port.isMulti,
                    isInput = port.isInput
                });
            }

            return node;
        }

        public PortReflectionData GetPortByName(string name)
        {
            return ports.Find((port) => port.name == name);
        }

        public override string ToString()
        {
            var inputs = new List<string>();
            var outputs = new List<string>();

            foreach (var port in ports)
            {
                if (port.isInput) inputs.Add(port.name);
                else if (!port.isEditable) outputs.Add(port.name);
            }

            return $"<{name}, IN: {string.Join(", ", inputs)}, OUT: {string.Join(", ", outputs)}>";
        }
    }
    
    public static class NodeReflection
    {
        private static Dictionary<string, NodeReflectionData> k_NodeTypes = null;
        
        /// <summary>
        /// Mapping between an AbstractNode type (key) and a custom editor type (value)
        /// </summary>
        private static Dictionary<Type, Type> k_NodeEditors = null;
        
        /// <summary>
        /// Retrieve reflection data for a given node class type
        /// </summary>
        public static NodeReflectionData GetNodeType(Type type)
        {
            return GetReflectionData(type.FullName);
        }

        public static NodeReflectionData GetReflectionData(string classFullName, string method)
        {
            return GetReflectionData($"{classFullName}|{method}");
        }

        static NodeReflectionData GetReflectionData(string key)
        {
            var types = GetNodeTypes();
            if (types.ContainsKey(key))
            {
                return types[key];
            }

            return null;
        }

        /// <summary>
        /// Get all types derived from the base node
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, NodeReflectionData> GetNodeTypes()
        {
            // Load cache if we got it
            if (k_NodeTypes != null)
            {
                return k_NodeTypes;
            }

            var baseType = typeof(AbstractNode);
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var nodes = new Dictionary<string, NodeReflectionData>();

            foreach (var assembly in assemblies)
            {
                foreach (var t in assembly.GetTypes())
                {
                    if (!t.IsAbstract && baseType.IsAssignableFrom(t))
                    {
                        // Aggregate [Node] inherited from baseType
                        var attr = t.GetCustomAttribute<NodeAttribute>();
                        if (attr != null)
                        {
                            nodes[t.FullName] = LoadClassReflection(t, attr);
                        }
                    }
                }
            }
        
            k_NodeTypes = nodes;
            return k_NodeTypes;
        }

        /// <summary>
        /// Extract <c>NodeReflectionData</c> from class reflection, attributes, and fields.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        static NodeReflectionData LoadClassReflection(Type type, NodeAttribute nodeAttr)
        {
            string name = nodeAttr.name ?? ObjectNames.NicifyVariableName(type.Name);
            string path = nodeAttr.module;
            
            var node = new NodeReflectionData()
            {
                type = type,
                path = path?.Split('/'),
                name = name,
                help = nodeAttr.help
            };

            node.AddFieldsFromClass(type);
            return node;
        }
        
        public static Type GetNodeEditorType(Type type)
        {
            if (k_NodeEditors == null)
            {
                LoadNodeEditorTypes();
            }
            
            k_NodeEditors.TryGetValue(type, out Type editorType);
            if (editorType != null)
            {
                return editorType;
            }

            // If it's not found, go up the inheritance tree until we find one
            while (type != typeof(AbstractNode))
            {
                type = type.BaseType;
                
                k_NodeEditors.TryGetValue(type, out editorType);
                if (editorType != null)
                {
                    return editorType;
                }
            }

            // Default to the base node editor
            return typeof(NodeView);
        }

        /// <summary>
        /// Load and cache a mapping between AbstractNode classes and their 
        /// NodeView editor equivalent, if a custom editor has been defined.
        /// </summary>
        static void LoadNodeEditorTypes()
        {
            var baseType = typeof(NodeView);
            var types = new List<Type>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            
            foreach (var assembly in assemblies)
            {
                try
                {
                    types.AddRange(assembly.GetTypes().Where(
                        (t) => !t.IsAbstract && baseType.IsAssignableFrom(t)
                    ).ToArray());
                } 
                catch (ReflectionTypeLoadException) { }
            }
            
            var nodeEditors = new Dictionary<Type, Type>();
            foreach (var t in types) 
            {
                // We only look at direct attributes here for associations.
                // GetNodeEditorType() handles walking up the inheritance tree.
                var attrs = t.GetCustomAttributes<CustomNodeViewAttribute>(false);
                foreach (var attr in attrs)
                {
                    nodeEditors[attr.nodeType] = t;
                }
            }

            k_NodeEditors = nodeEditors;
        }
    }
}
