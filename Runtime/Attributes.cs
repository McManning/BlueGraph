using System;

namespace BlueGraph
{
    /// <summary>
    /// Metadata for a Node available to Graphs
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class NodeAttribute : Attribute
    {
        /// <summary>
        /// Display name of the node
        /// </summary>
        public string name;

        /// <summary>
        /// Tooltip help content displayed for the node
        /// </summary>
        public string help;

        /// <summary>
        /// Slash-delimited module path to categorize this node under
        /// </summary>
        public string module;

        public NodeAttribute(string name = null)
        {
            this.name = name;
        }
    }
    
    /// <summary>
    /// Metadata for an input slot on a Node
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class InputAttribute : Attribute
    {
        /// <summary>
        /// Display name of the input slot
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
    /// Metadata for an output slot on a Node
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class OutputAttribute : Attribute
    {
        /// <summary>
        /// Display name of the output slot
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
    /// Display an inline editor for this field on the Node
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class EditableAttribute : Attribute
    {
        /// <summary>
        /// Display name of the editable field
        /// </summary>
        public string name;
        
        public EditableAttribute(string name = null)
        {
            this.name = name;
        }
    }
    
    /// <summary>
    /// Mark a static class as containing a suite of FuncNode methods
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class FuncNodeModuleAttribute : Attribute
    {
        /// <summary>
        /// Default module path for all contained FuncNode methods.
        /// Can be slash-delimited to denote submodules. 
        /// </summary>
        public string path;
        
        public FuncNodeModuleAttribute(string path = null)
        {
            this.path = path;    
        }
    }

    /// <summary>
    /// Override default settings from inherited FuncNodeModule
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class FuncNodeAttribute : Attribute
    {
        /// <summary>
        /// Custom display name of the FuncNode
        /// </summary>
        public string name;

        /// <summary>
        /// Optional module path to override FuncNodeModule.
        /// Can be slash-delimited to denote submodules.
        /// </summary>
        public string module;

        /// <summary>
        /// Display name of the return value. Defaults to "Result"
        /// </summary>
        public string returnName;

        /// <summary>
        /// Class to instantiate while wrapping each function
        /// as a node on the graph. Must be a type that inherits
        /// from `FuncNode`. 
        /// </summary>
        public Type classType;

        public FuncNodeAttribute(string name = null)
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
}
