
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
        
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var tree = new List<SearchTreeEntry>();
        
            // First item is the title of the window
            tree.Add(new SearchTreeGroupEntry(new GUIContent("Add Node"), 0));
        
            // TODO: Clever nested grouping here. Just grab it from .. wherever. 
            // Everyone does the same thing.
        
            // The rest are our available nodes. TODO: Group and such.
            var group = new SearchTreeGroupEntry(new GUIContent("Everything lol"));
            group.level = 1;
            tree.Add(group);
            
            var nodes = NodeReflection.GetNodeTypes();
            foreach (var node in nodes.Values)
            {
                var entry = new SearchTreeEntry(new GUIContent(node.name));
                entry.level = 2;
                entry.userData = node.type;
                tree.Add(entry);
            }
        
            // TODO: Context sensitive based on `connectedPort`
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
