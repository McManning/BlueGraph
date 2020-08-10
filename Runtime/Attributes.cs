using System;

namespace BlueGraph
{
    /// <summary>
    /// A node that can be added to a Graph
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class NodeAttribute : Attribute
    {
        /// <summary>
        /// Display name of the node. 
        /// 
        /// If not supplied, this will be inferred based on the class name.
        /// </summary>
        public string name;

        /// <summary>
        /// Tooltip help content displayed for the node
        /// </summary>
        public string help;

        /// <summary>
        /// Slash-delimited directory path to categorize this node in the search window.
        /// </summary>
        public string path;

        /// <summary>
        /// Can this node be deleted from the graph
        /// </summary>
        public bool deletable = true;
        
        public NodeAttribute(string name = null)
        {
            this.name = name;
        }
    }
    
    /// <summary>
    /// Tags associated with a Node. Can be used by a Graph's <c>[IncludeTags]</c>
    /// attribute to restrict what nodes can be added to the graph. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class TagsAttribute : Attribute
    {
        public string[] tags;

        public TagsAttribute(params string[] tags)
        {
            this.tags = tags;
        }
    }
    
    /// <summary>
    /// An input port exposed on a Node
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class InputAttribute : Attribute
    {
        /// <summary>
        /// Display name of the input slot.
        /// 
        /// If not supplied, this will be inferred based on the field name.
        /// </summary>
        public string name;

        /// <summary>
        /// Can this input accept multiple connections at once
        /// </summary>
        public bool multiple = false;

        /// <summary>
        /// Can this input value be directly modified when there are no connections
        /// </summary>
        public bool editable = true;
        
        public InputAttribute(string name = null)
        {
            this.name = name;
        }
    }
    
    /// <summary>
    /// An output port exposed on a Node.
    /// 
    /// This can either be defined on the class or associated with a specific field. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Class, AllowMultiple = true)]
    public class OutputAttribute : Attribute
    {
        /// <summary>
        /// Display name of the output slot.
        /// 
        /// If not supplied, this will be inferred based on the field name.
        /// </summary>
        public string name;
        
        /// <summary>
        /// Can this output go to multiple connections at once
        /// </summary>
        public bool multiple = true;

        /// <summary>
        /// If defined as a class attribute, this is the output type.
        /// 
        /// When defined on a field, the output will automatically be inferred by the field.
        /// </summary>
        public Type type;

        public OutputAttribute(string name = null, Type type = null)
        {
            this.name = name;
            this.type = type;
        }
    }

    /// <summary>
    /// A field that can be edited directly from within the Canvas on a Node
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class EditableAttribute : Attribute
    {
        /// <summary>
        /// Display name of the editable field. 
        /// 
        /// If not supplied, this will be inferred based on the field name.
        /// </summary>
        public string name;
        
        public EditableAttribute(string name = null)
        {
            this.name = name;
        }
    }
    
    /// <summary>
    /// Supported node tags for a given Graph. 
    /// 
    /// If defined, only nodes with a <c>[Tags]</c> attribute including 
    /// one or more of these tags may be added to the Graph. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class IncludeTagsAttribute : Attribute
    {
        public string[] tags;

        public IncludeTagsAttribute(params string[] tags)
        {
            this.tags = tags;
        }
    }
    
    /// <summary>
    /// Mark a node as deprecated and automatically migrate instances
    /// to a new class when encountered in the editor.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class DeprecatedAttribute : Attribute
    {
        public Type replaceWith;
    }

    /// <summary>
    /// Mark a class inherited from <c>NodeView</c> as the primary view
    /// for a specific type of node. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class CustomNodeViewAttribute : Attribute
    {
        public Type nodeType;

        public CustomNodeViewAttribute(Type nodeType)
        {
            this.nodeType = nodeType; 
        }
    }
}
