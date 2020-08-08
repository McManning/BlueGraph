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
        /// Slash-delimited module path to categorize this node for searches 
        /// and restrict what Graphs it can be instantiated onto.
        /// </summary>
        public string module;

        public NodeAttribute(string name = null)
        {
            this.name = name;
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
    /// An output port exposed on a Node
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
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

        public OutputAttribute(string name = null)
        {
            this.name = name;
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
    /// Supported module paths for a given Graph. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class IncludeModulesAttribute : Attribute
    {
        public string[] modules;

        public IncludeModulesAttribute(params string[] modules)
        {
            this.modules = modules;
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
}
