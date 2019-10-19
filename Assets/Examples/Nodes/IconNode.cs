using System;
using UnityEngine;
using BlueGraph;

namespace BlueGraphExamples
{
    /// <summary>
    /// Custom icon resource to display within a node
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class NodeIconAttribute : Attribute
    {
        public string iconName;
        public bool showTitle;

        public NodeIconAttribute(string iconName = null)
        {
            this.iconName = iconName;
        }
    }
    
    /// <summary>
    /// Base class to use the IconNodeView for icon-only nodes
    /// </summary>
    public class IconNode : AbstractNode
    {

    }
}
