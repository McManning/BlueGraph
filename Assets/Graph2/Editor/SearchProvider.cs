
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;

namespace Graph2
{
    public class SearchProvider : ScriptableObject, ISearchWindowProvider
    {
        public GraphViewElement graphView;
        public PortView connectedPort;
        
        class SearchGroup
        {
            public SearchGroup(int depth)
            {
                this.depth = depth;
            }
            
            public int depth;
            public SortedDictionary<string, SearchGroup> subgroups = new SortedDictionary<string, SearchGroup>();
            public List<NodeReflectionData> nodes = new List<NodeReflectionData>();

            public void AddToTree(List<SearchTreeEntry> tree)
            {
                SearchTreeEntry entry;
                
                // Add subgroups
                foreach (var group in subgroups)
                {
                    entry = new SearchTreeGroupEntry(new GUIContent(group.Key));
                    entry.level = depth;
                    tree.Add(entry);
                    group.Value.AddToTree(tree);
                }

                // Add nodes
                foreach (var node in nodes)
                { 
                    entry = new SearchTreeEntry(new GUIContent(node.name));
                    entry.level = depth;
                    entry.userData = node.type;
                    tree.Add(entry);
                }
            }
        }

        protected bool IsCompatibleWithConnectedPort(NodeReflectionData node)
        {
            if (connectedPort.direction == Direction.Output)
            {
                return node.HasInputOfType(connectedPort.portType);
            }

            return node.HasOutputOfType(connectedPort.portType);
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var tree = new List<SearchTreeEntry>();
        
            // First item is the title of the window
            tree.Add(new SearchTreeGroupEntry(new GUIContent("Add Node"), 0));
     
            // Construct a tree of available nodes by path
            var nodes = NodeReflection.GetNodeTypes();

            var groups = new SearchGroup(1);
            foreach (var node in nodes.Values)
            {
                // If we're coming from a port, make sure to only add nodes that accept
                // an input (or output) that's compatible. 
                if (connectedPort == null || IsCompatibleWithConnectedPort(node))
                {
                    var path = node.path;
                    var group = groups;
                    if (path != null)
                    {
                        for (int i = 0; i < path.Length; i++)
                        {
                            if (!group.subgroups.ContainsKey(path[i]))
                            {
                                group.subgroups.Add(path[i], new SearchGroup(group.depth + 1));
                            }

                            group = group.subgroups[path[i]];
                        }
                    }
                    
                    group.nodes.Add(node);
                }
            }
            
            groups.AddToTree(tree);
            
            // TODO: Blacklisting certain nodes by some other context (e.g. two independent graph systems)
        
            return tree;
        }
        
        public bool OnSelectEntry(SearchTreeEntry entry, SearchWindowContext context)
        {
            graphView.CreateNode(
                entry.userData as Type, 
                context.screenMousePosition, 
                connectedPort
            );

            return true;
        }
    }
}
