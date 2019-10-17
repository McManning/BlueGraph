using System;

namespace BlueGraph
{
    [AttributeUsage(AttributeTargets.Class)]
    public class NodeAttribute : Attribute
    {
        public string name;
        public string help;
        public string category;

        public NodeAttribute(string name = null)
        {
            this.name = name;
        }
    }
    
    [AttributeUsage(AttributeTargets.Field)]
    public class InputAttribute : Attribute
    {
        public string name;
        public bool multiple = false;
        public bool editable = true;
        
        public InputAttribute(string name = null)
        {
            this.name = name;
        }
    }
    
    [AttributeUsage(AttributeTargets.Field)]
    public class OutputAttribute : Attribute
    {
        public string name;
        
        public OutputAttribute(string name = null)
        {
            this.name = name;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class EditableAttribute : Attribute
    {
        public string name;
        
        public EditableAttribute(string name = null)
        {
            this.name = name;
        }
    }
}
