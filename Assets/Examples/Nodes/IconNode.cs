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
        public string name;

        public NodeIconAttribute(string name)
        {
            this.name = name;
        }
    }
    
    /// <summary>
    /// Base class to use the IconNodeView for icon-only nodes
    /// </summary>
    public class IconNode : AbstractNode
    {

    }
}
