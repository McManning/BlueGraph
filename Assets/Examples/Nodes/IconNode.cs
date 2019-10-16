using System;
using UnityEngine;
using BlueGraph;

namespace BlueGraphExamples
{
    /// <summary>
    /// Custom attribute to let the user define an icon per-node
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class NodeIconAttribute : Attribute
    {
        public string Name;

        public NodeIconAttribute(string name)
        {
            Name = name;
        }
    }
    
    /// <summary>
    /// Base class to use the IconNodeView for icon-only nodes
    /// </summary>
    public class IconNode : AbstractNode
    {

    }
}
