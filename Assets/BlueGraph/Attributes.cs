using System;

namespace BlueGraph
{
    [AttributeUsage(AttributeTargets.Class)]
    public class NodeAttribute : Attribute
    {
        public string Name;
        public string Tooltip;

        public NodeAttribute(string name = null)
        {
            Name = name;
        }
    }
    
    [AttributeUsage(AttributeTargets.Field)]
    public class InputAttribute : Attribute
    {
        public string Name;
        public bool Multiple = false;
        public bool Editable = true;
        
        public InputAttribute(string name = null)
        {
            Name = name;
        }
    }
    
    [AttributeUsage(AttributeTargets.Field)]
    public class OutputAttribute : Attribute
    {
        public string Name;
        
        public OutputAttribute(string name = null)
        {
            Name = name;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class EditableAttribute : Attribute
    {
        public string Name;
        
        public EditableAttribute(string name = null)
        {
            Name = name;
        }
    }
}
