using UnityEngine;
using System.Collections;
using UnityEditor.Experimental.GraphView;
using System.Collections.Generic;

namespace Graph2
{
    /// <summary>
    /// Wrapper over GraphView.Group to pass changes onto 
    /// the serializable asset
    /// </summary>
    public class GroupView : Group
    {
        public NodeGroup nodeGroup;

        public GroupView(NodeGroup group)
        {
            nodeGroup = group;
            title = group.title;
        }

        protected override void OnElementsAdded(IEnumerable<GraphElement> elements)
        {
            foreach (var element in elements)
            {
                Debug.Log("on added: " + element.title);
                if (element is NodeView)
                {
                    var nodeView = element as NodeView;

                    // TODO: When a group is loaded from persistence,
                    // each node in that graph will get re-iterate and 
                    // added again here. 
                    if (!nodeGroup.nodes.Contains(nodeView.NodeData))
                    {
                        nodeGroup.nodes.Add(nodeView.NodeData);
                    }

                    // TODO: De-associate that node from other groups.
                    // Happens automatically in the editor but is it safe?
                }
            }

            base.OnElementsAdded(elements);
        }

        protected override void OnElementsRemoved(IEnumerable<GraphElement> elements)
        {   
            foreach (var element in elements)
            {
                Debug.Log("on removed: " + element.title);
                if (element is NodeView)
                {
                    var nodeView = element as NodeView;
                    
                    nodeGroup.nodes.Remove(nodeView.NodeData);
                }
            }

            base.OnElementsRemoved(elements);
        }

        /// <summary>
        /// Force the group to always have a name, even if they clear it.
        /// </summary>
        protected override void OnGroupRenamed(string oldName, string newName)
        {
            base.OnGroupRenamed(oldName, newName);
            
            if (newName.Length < 1)
            {
                newName = "New Group";
            }

            nodeGroup.title = newName;
            title = newName;
        }
    }
}
