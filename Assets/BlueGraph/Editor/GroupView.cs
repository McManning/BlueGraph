
using UnityEditor.Experimental.GraphView;
using System.Collections.Generic;
using BlueGraph;

namespace BlueGraphEditor
{
    /// <summary>
    /// Wrapper over GraphView.Group to pass changes onto 
    /// the serializable asset
    /// </summary>
    public class GroupView : Group, ICanDirty
    {
        public NodeGroup target;

        public GroupView(NodeGroup group)
        {
            target = group;
            title = group.title;
        }

        public void OnDirty()
        {
            
        }

        public void OnUpdate()
        {
            // TODO: Update position. It doesn't really work because 
            // the nodes still maintain their original position, even after moved. 
        }

        protected override void OnElementsAdded(IEnumerable<GraphElement> elements)
        {
            foreach (var element in elements)
            {
                if (element is NodeView)
                {
                    var node = (element as NodeView).target;
                    if (!target.nodes.Contains(node))
                    {
                        target.nodes.Add(node);
                    }
                }
            }

            base.OnElementsAdded(elements);
        }

        protected override void OnElementsRemoved(IEnumerable<GraphElement> elements)
        {   
            foreach (var element in elements)
            {
                if (element is NodeView)
                {
                    var node = (element as NodeView).target;
                    target.nodes.Remove(node);
                }
            }

            base.OnElementsRemoved(elements);
        }

        protected override void OnGroupRenamed(string oldName, string newName)
        {
            base.OnGroupRenamed(oldName, newName);
            
            // Force the group to have a title if cleared. This avoids awkward
            // interactions when trying to move the group or add a title later.
            if (newName.Length < 1)
            {
                newName = "New Group";
            }

            target.title = newName;
            title = newName;
        }
    }
}
