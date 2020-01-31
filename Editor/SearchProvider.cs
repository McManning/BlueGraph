
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using System.Linq;

namespace BlueGraph.Editor
{
    public class SearchProvider : ScriptableObject, ISearchWindowProvider
    {
        public CanvasView target;
        public PortView sourcePort;

        /// <summary>
        /// Whitelist of node modules to include in search results.
        /// All submodules will be included (e.g. "Foo" will include "Foo/Bar", "Foo/Fizz/Buzz", etc).
        /// </summary>
        public List<string[]> modules = new List<string[]>();
        
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
                    entry.userData = node;
                    tree.Add(entry);
                }
            }
        }

        protected bool IsCompatibleWithSourcePort(NodeReflectionData node)
        {
            if (sourcePort.direction == Direction.Output)
            {
                return node.HasInputOfType(sourcePort.portType);
            }

            return node.HasOutputOfType(sourcePort.portType);
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var tree = new List<SearchTreeEntry>();
        
            // First item is the title of the window
            tree.Add(new SearchTreeGroupEntry(new GUIContent("Add Node"), 0));

            // TODO: Hooks for custom top level pieces (Comments, new variables, etc)
     
            // Construct a tree of available nodes by module path
            var nodes = NodeReflection.GetNodeTypes();

            var groups = new SearchGroup(1);
            foreach (var node in nodes.Values)
            {
                var path = node.path;

                // Skip the node if it the module isn't whitelisted
                if (!IsInSupportedModule(path)) continue;

                // If we're coming from a port, make sure to only add nodes that accept
                // an input (or output) that's compatible. 
                if (sourcePort == null || IsCompatibleWithSourcePort(node))
                {
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

            return tree;
        }

        /// <summary>
        /// Returns true if the input path is prefixed by one or more registered module paths.
        /// </summary>
        private bool IsInSupportedModule(string[] path)
        {
            // Everything in global namespace is allowed.
            if (path == null) return true;
            
            foreach (var module in modules)
            {
                if (path.Length < module.Length) continue;
                
                bool match = true;
                for (int i = 0; i < module.Length && match; i++)
                {
                    match = path[i] == module[i];
                }

                if (match)
                {
                    return true;
                }
            }

            return false;
        }

        public bool OnSelectEntry(SearchTreeEntry entry, SearchWindowContext context)
        {
            target.AddNodeFromReflectionData(
                entry.userData as NodeReflectionData, 
                context.screenMousePosition, 
                sourcePort
            );

            return true;
        }
    }
}
