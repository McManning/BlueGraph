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

    public class NodeReflectionData
    {
        /// <summary>
        /// Class type to instantiate for the node 
        /// </summary>
        public Type type;

        /// <summary>
        /// The bind method for FuncNodes
        /// </summary>
        public MethodInfo method;

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
            foreach (var port in ports)
            {
                if (!port.isInput) continue;
       
                // Cast direction type -> port input
                if (type.IsCastableTo(port.type, true))
                {
                    return true;
                }
            }

            return false;
        }

        public bool HasOutputOfType(Type type)
        {
            // return ports.Count((port) => !port.isInput && port.type == type) > 0;
            
            foreach (var port in ports)
            {
                if (port.isInput) continue;
       
                // Cast direction port output -> type
                if (port.type.IsCastableTo(type, true))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Create a node instance from the reflected type data
        /// </summary>
        public AbstractNode CreateInstance()
        {
            AbstractNode node = ScriptableObject.CreateInstance(type) as AbstractNode;
            node.RegenerateGuid();
            node.name = name;
            
            // Setup ports
            foreach (var port in ports)
            {
                // Now it's basically everything from reflection.
                // TODO Get rid of reflection?
                node.AddPort(new NodePort() {
                    node = node,
                    portName = port.portName,
                    isMulti = port.isMulti,
                    isInput = port.isInput,
                    type = port.type,
                    fieldName = port.fieldName
                });
            }
            
            // If we spawned a FuncNode, bind it to the method. 
            if (method != null && node is FuncNode func)
            {
                func.CreateDelegate(method);
            }

            return node;
        }
    }
    
    /// <summary>
    /// Suite of reflection methods and caching for retrieving available
    /// graph nodes and their associated custom editors
    /// </summary>
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

        public static NodeReflectionData GetReflectionData(string key)
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
            var moduleTypes = new List<Type>();

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
                    else if (t.GetCustomAttribute<FuncNodeModuleAttribute>() != null)
                    {
                        // Aggregate classes that are [FuncNodeModule]
                        moduleTypes.Add(t);
                    }
                }
            }
        
            // Run through modules and add tagged methods as FuncNodes
            foreach (var type in moduleTypes) 
            {
                var attr = type.GetCustomAttribute<FuncNodeModuleAttribute>();
                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static);

                foreach (var method in methods)
                {
                    nodes[$"{type.FullName}|{method.Name}"] = LoadMethodReflection(method, attr);
                }
            }
        
            k_NodeTypes = nodes;
            return k_NodeTypes;
        }

        private static NodeReflectionData LoadMethodReflection(MethodInfo method, FuncNodeModuleAttribute moduleAttr)
        {    
            var attr = method.GetCustomAttribute<FuncNodeAttribute>();
            string name = attr?.name ?? ObjectNames.NicifyVariableName(method.Name);

            // FuncNode.module can override FuncNodeModule.path. 
            string path = attr?.module ?? moduleAttr.path;
        
            var node = new NodeReflectionData()
            {
                type = typeof(FuncNode),
                path = path?.Split('/'),
                name = name,
                tooltip = "TODO!",
                method = method
            };
            
            ParameterInfo[] parameters = method.GetParameters();
            
            foreach (var parameter in parameters)
            {
                node.ports.Add(new PortReflectionData() {
                    type = parameter.IsOut ? 
                        parameter.ParameterType.GetElementType() :
                        parameter.ParameterType,
                    portName = ObjectNames.NicifyVariableName(parameter.Name),
                    fieldName = parameter.Name,
                    isMulti = parameter.IsOut,
                    isInput = !parameter.IsOut
                });
            }
            
            // Add an output port for the return value if non-void
            if (method.ReturnType != typeof(void))
            {
                node.ports.Add(new PortReflectionData() {
                    type = method.ReturnType,
                    portName = "Result",
                    fieldName = null,
                    isMulti = true,
                    isInput = false
                });
            }
            
            return node;
        }

        /// <summary>
        /// Extract NodeField information from class reflection + attributes
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static NodeReflectionData LoadClassReflection(Type type, NodeAttribute nodeAttr)
        {
            string name = nodeAttr.name ?? type.Name;
            string path = nodeAttr.module;
            
            var node = new NodeReflectionData()
            {
                type = type,
                path = path?.Split('/'),
                name = name,
                tooltip = nodeAttr.help
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
                            portName = attr.name ?? ObjectNames.NicifyVariableName(fields[i].Name),
                            fieldName = fields[i].Name,
                            isInput = true,
                            isMulti = attr.multiple,
                            isEditable = attr.editable
                        });
                    }
                    else if (attribs[j] is OutputAttribute)
                    {
                        var attr = attribs[j] as OutputAttribute;

                        node.ports.Add(new PortReflectionData()
                        {
                            type = fields[i].FieldType,
                            portName = attr.name ?? ObjectNames.NicifyVariableName(fields[i].Name),
                            fieldName = fields[i].Name,
                            isInput = false,
                            isMulti = attr.multiple,
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
            
            // TODO: Move or collapse.
            // If node reflection data is needed outside the editor, maybe hold onto this. 

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
