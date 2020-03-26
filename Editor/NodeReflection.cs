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
    public class PortReflectionData
    {
        public Type Type => field.FieldType;

        public FieldInfo field;

        public string portName;

        public bool acceptsMultipleConnections;
        public bool isInput;
        public bool isEditable; // TODO: Rename?
    }

    public class EditableReflectionData
    {
        public Type Type => field.FieldType;

        public string displayName;
        public FieldInfo field;
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
    
        public List<FieldInfo> fields = new List<FieldInfo>();

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
                if (type.IsCastableTo(port.Type, true))
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
                if (port.Type.IsCastableTo(type, true))
                {
                    return true;
                }
            }

            return false;
        }
        
        /// <summary>
        /// Add ports based on attributes on the class fields 
        /// </summary>
        /// <param name="type"></param>
        public void AddPortsFromClass(Type type)
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
                    if (attribs[j] is InputAttribute)
                    {
                        var attr = attribs[j] as InputAttribute;

                        ports.Add(new PortReflectionData()
                        {
                            portName = attr.name ?? ObjectNames.NicifyVariableName(fields[i].Name),
                            field = fields[i],
                            isInput = true,
                            acceptsMultipleConnections = attr.multiple,
                            isEditable = attr.editable
                        });
                    }
                    else if (attribs[j] is OutputAttribute)
                    {
                        var attr = attribs[j] as OutputAttribute;
                        
                        ports.Add(new PortReflectionData()
                        {
                            portName = attr.name ?? ObjectNames.NicifyVariableName(fields[i].Name),
                            field = fields[i],
                            isInput = false,
                            acceptsMultipleConnections = attr.multiple,
                            isEditable = false
                        });
                    }
                    else if (attribs[j] is EditableAttribute)
                    {
                        var attr = attribs[j] as EditableAttribute;
                        
                        editables.Add(new EditableReflectionData()
                        {
                            displayName = attr.name ?? ObjectNames.NicifyVariableName(fields[i].Name),
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
            
            // Setup ports
            foreach (var port in ports)
            {
                // Now it's basically everything from reflection.
                // TODO Get rid of reflection?
                var nodePort = new Port {
                    Type = port.Type,
                    node = node,
                    name = port.portName,
                    acceptsMultipleConnections = port.acceptsMultipleConnections,
                    isInput = port.isInput
                };

                // Only pass the field name along if we allow
                // inline editing of the port
                if (port.isEditable)
                {
                    nodePort.fieldName = port.field.Name;
                }

                node.AddPort(nodePort);
            }

            return node;
        }
        
        /// <summary>
        /// Create a VisualElement for a port's inline editor based on the field data type
        /// </summary>
        /// <param name="view"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public VisualElement GetControlElement(NodeView view, string fieldName)
        {
            var fieldInfo = fields.Find((field) => field.Name == fieldName);
            if (fieldInfo == null)
            {
                return null;
            }
            
            // TODO: Rip this out from this point and put somewhere else that's pretty + extendable.

            // This mess is similar to what Unity is doing for ShaderGraph. It's not great.
            // But the automatic alternatives depend on SerializableObject which is a performance bottleneck.
            // Ref: https://github.com/Unity-Technologies/ScriptableRenderPipeline/blob/master/com.unity.shadergraph/Editor/Drawing/Controls/DefaultControl.cs
            
            // Builtin unity type editors
            if (fieldInfo.FieldType == typeof(bool))
                return CreateControlElement<Toggle, bool>(view, fieldInfo);
            
            if (fieldInfo.FieldType == typeof(int))
                return CreateControlElement<IntegerField, int>(view, fieldInfo);
            
            if (fieldInfo.FieldType == typeof(float))
                return CreateControlElement<FloatField, float>(view, fieldInfo);
            
            if (fieldInfo.FieldType == typeof(string))
                return CreateControlElement<TextField, string>(view, fieldInfo);
            
            if (fieldInfo.FieldType == typeof(Rect))
                return CreateControlElement<RectField, Rect>(view, fieldInfo);
            
            if (fieldInfo.FieldType == typeof(Color))
                return CreateControlElement<ColorField, Color>(view, fieldInfo);
            
            if (fieldInfo.FieldType == typeof(Vector2))
                return CreateControlElement<Vector2Field, Vector2>(view, fieldInfo);
            
            if (fieldInfo.FieldType == typeof(Vector3))
                return CreateControlElement<Vector3Field, Vector3>(view, fieldInfo);
            
            if (fieldInfo.FieldType == typeof(Vector4))
                return CreateControlElement<Vector4Field, Vector4>(view, fieldInfo);
            
            if (fieldInfo.FieldType == typeof(Matrix4x4))
                return CreateControlElement<Vector4Field, Vector4>(view, fieldInfo);
            
            if (fieldInfo.FieldType == typeof(Gradient))
                return CreateControlElement<GradientField, Gradient>(view, fieldInfo);
            
            if (fieldInfo.FieldType == typeof(AnimationCurve))
                return CreateControlElement<CurveField, AnimationCurve>(view, fieldInfo);
            
            if (typeof(Enum).IsAssignableFrom(fieldInfo.FieldType))
                return CreateControlElement<EnumField, Enum>(view, fieldInfo); 
    
            // TODO: EnumFlags/Masks

            if (typeof(Object).IsAssignableFrom(fieldInfo.FieldType))
                return CreateControlElement<ObjectField, Object>(view, fieldInfo);

            // TODO: Specialized common types. Transform, Rotation, Texture2D, etc.

            return null;
        }
        
        /// <summary>
        /// Generic factory for instantiating and configuring builtin Unity controls for value types.
        /// 
        /// The control will be created and bound to the given NodeView and its associated target node. 
        /// </summary>
        VisualElement CreateControlElement<TField, TType>(NodeView view, FieldInfo fieldInfo) where TField: BaseField<TType>, new()
        {
            var field = new TField();
            field.SetValueWithoutNotify((TType)fieldInfo.GetValue(view.target));
            field.RegisterValueChangedCallback((change) =>
            {
                fieldInfo.SetValue(view.target, change.newValue);
                view.OnPropertyChange();
            });

            return field;
        }
        
        public override string ToString()
        {
            var inputs = new List<string>();
            var outputs = new List<string>();

            foreach (var port in ports)
            {
                if (port.isInput) inputs.Add(port.portName);
                else if (!port.isEditable) outputs.Add(port.portName);
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
            /*foreach (var type in moduleTypes) 
            {
                var attr = type.GetCustomAttribute<FuncNodeModuleAttribute>();
                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static);

                foreach (var method in methods)
                {
                    nodes[$"{type.FullName}|{method.Name}"] = LoadMethodReflection(method, attr);
                }
            }*/
            
            k_NodeTypes = nodes;
            return k_NodeTypes;
        }

        /*
        static NodeReflectionData LoadMethodReflection(MethodInfo method, FuncNodeModuleAttribute moduleAttr)
        {    
            var attr = method.GetCustomAttribute<FuncNodeAttribute>();
            var name = attr?.name ?? ObjectNames.NicifyVariableName(method.Name);
            var returnPortName = attr?.returnName ?? "Result";
            
            // FuncNode.module can override FuncNodeModule.path. 
            var path = attr?.module ?? moduleAttr.path;
            
            // FuncNode.classType can override default class type
            var classType = attr?.classType; // ?? typeof(FuncNode);
            
            var node = new NodeReflectionData()
            {
                type = classType,
                path = path?.Split('/'),
                name = name,
                tooltip = "TODO!",
                method = method
            };
            
            var parameters = method.GetParameters();
            foreach (var parameter in parameters)
            {
                node.ports.Add(new PortReflectionData() {
                    type = parameter.IsOut ? 
                        parameter.ParameterType.GetElementType() :
                        parameter.ParameterType,
                    portName = ObjectNames.NicifyVariableName(parameter.Name),
                    fieldName = parameter.Name,
                    acceptsMultipleConnections = parameter.IsOut,
                    isInput = !parameter.IsOut
                });
            }
            
            // Add an output port for the return value if non-void
            if (method.ReturnType != typeof(void))
            {
                node.ports.Add(new PortReflectionData() {
                    type = method.ReturnType,
                    portName = returnPortName,
                    fieldName = null,
                    acceptsMultipleConnections = true,
                    isInput = false
                });
            }

            // Merge in any reflection data from the wrapper class itself
            // Specifically if it contains additional ports to include
            node.AddPortsFromClass(classType);

            return node;
        }*/

        /// <summary>
        /// Extract NodeField information from class reflection + attributes
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
                tooltip = nodeAttr.help
            };

            node.AddPortsFromClass(type);
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
        static void LoadNodeEditorTypes()
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
                var attrs = t.GetCustomAttributes<CustomNodeViewAttribute>();
                foreach (var attr in attrs)
                {
                    nodeEditors[attr.nodeType] = t;
                }
            }
            
            k_NodeEditors = nodeEditors;
        }
    }
}
