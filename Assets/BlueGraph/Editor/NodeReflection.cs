using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using BlueGraph;
using UnityEngine;

namespace BlueGraphEditor
{
    public class PortReflectionData
    {
        public Type type;
        public string portName;
        public string fieldName;

        public bool isMulti;
        public bool isInput;
        public bool isEditable; // TODO: Rename
    }

    public class EditableReflectionData
    {
        public Type type;
        public string fieldName;
    }

    /// <summary>
    /// Suite of reflection methods and caching for retrieving available
    /// graph nodes and their associated custom editors
    /// </summary>
    public class NodeReflectionData
    {
        public Type type;

        /// <summary>
        /// Full path name for grouping nodes excluding the last part (name)
        /// </summary>
        public string[] path;

        /// <summary>
        /// Human-readable display name of the node. Will come from the last
        /// part of the path parsed out of node information - or be the class name.
        /// </summary>
        public string name;

        /// <summary>
        /// Tooltip content for node usage instructions
        /// </summary>
        public string tooltip;

        public List<PortReflectionData> ports = new List<PortReflectionData>();
        public List<EditableReflectionData> editables = new List<EditableReflectionData>();
    
        public bool HasSingleOutput()
        {
            return ports.Count((port) => !port.isInput) < 2;
        }

        public bool HasInputOfType(Type type)
        {
            return ports.Count((port) => port.isInput && port.type == type) > 0;
        }

        public bool HasOutputOfType(Type type)
        {
            return ports.Count((port) => !port.isInput && port.type == type) > 0;
        }
    }

    public static class NodeReflection
    {
        private static Dictionary<Type, NodeReflectionData> k_NodeTypes = null;

        /// <summary>
        /// Mapping between an AbstractNode type (key) and a custom editor type (value)
        /// </summary>
        private static Dictionary<Type, Type> k_NodeEditors = null;
        
        /// <summary>
        /// Extract node type info for one node
        /// </summary>
        public static NodeReflectionData GetNodeType(Type type)
        {
            var types = GetNodeTypes();

            if (types.ContainsKey(type))
            {
                return types[type];
            }

            return null;
        }

        /// <summary>
        /// Get all types derived from the base node
        /// </summary>
        /// <returns></returns>
        public static Dictionary<Type, NodeReflectionData> GetNodeTypes()
        {
            // Load cache if we got it
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
        
            var nodes = new Dictionary<Type, NodeReflectionData>();
            foreach (var type in types) 
            {
                var attr = type.GetCustomAttribute<NodeAttribute>();
                if (attr != null)
                {
                    nodes[type] = LoadReflection(type, attr);
                }
            }
        
            k_NodeTypes = nodes;
            return k_NodeTypes;
        }

        /// <summary>
        /// Extract NodeField information from class reflection + attributes
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static NodeReflectionData LoadReflection(Type type, NodeAttribute nodeAttr)
        {
            string[] path = null;
            string name = type.Name;
            if (nodeAttr.Name != null) 
            {
                var stack = new Stack<string>(nodeAttr.Name.Split('/'));
                name = stack.Pop();
                path = stack.ToArray();
            }

            var node = new NodeReflectionData()
            {
                type = type,
                path = path,
                name = name,
                tooltip = nodeAttr.Help
            };

            var fields = new List<FieldInfo>(type.GetFields(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
            ));
        
            // Iterate through inherited private fields as well
            var temp = type;
            while ((temp = temp.BaseType) != typeof(AbstractNode))
            {
                fields.AddRange(temp.GetFields(BindingFlags.NonPublic | BindingFlags.Instance));
            }
        
            // Extract port and editable metadata from each tagged field
            var ports = new List<PortReflectionData>();
            for (int i = 0; i < fields.Count; i++)
            {
                object[] attribs = fields[i].GetCustomAttributes(true);
                for (int j = 0; j < attribs.Length; j++)
                {
                    if (attribs[j] is InputAttribute)
                    {
                        var attr = attribs[j] as InputAttribute;
                        
                        node.ports.Add(new PortReflectionData()
                        {
                            type = fields[i].FieldType,
                            portName = attr.Name ?? ObjectNames.NicifyVariableName(fields[i].Name),
                            fieldName = fields[i].Name,
                            isInput = true,
                            isMulti = attr.Multiple,
                            isEditable = attr.Editable
                        });
                    }
                    else if (attribs[j] is OutputAttribute)
                    {
                        var attr = attribs[j] as OutputAttribute;

                        node.ports.Add(new PortReflectionData()
                        {
                            type = fields[i].FieldType,
                            portName = attr.Name ?? ObjectNames.NicifyVariableName(fields[i].Name),
                            fieldName = fields[i].Name,
                            isInput = false,
                            isMulti = true,
                            isEditable = false
                        });
                    }
                    else if (attribs[j] is EditableAttribute)
                    {
                        var attr = attribs[j] as EditableAttribute;
                        
                        node.editables.Add(new EditableReflectionData()
                        {
                            type = fields[i].FieldType,
                            fieldName = fields[i].Name
                        });
                    }
                }
            }

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

            // If it's not found, go down the inheritance tree until we find one
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
        private static void LoadNodeEditorTypes()
        {
            var baseType = typeof(NodeView);
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
            
            var nodeEditors = new Dictionary<Type, Type>();
            foreach (var t in types) 
            {
                Debug.Log(t);
                var attr = t.GetCustomAttribute<CustomNodeViewAttribute>();
                if (attr != null)
                {
                    nodeEditors[attr.nodeType] = t;
                }
            }
            
            k_NodeEditors = nodeEditors;
        }
    }
}
