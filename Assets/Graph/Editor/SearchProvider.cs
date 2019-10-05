
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

/// <summary>
/// Provider that populates the search menu when finding nodes to add to the graph.
/// Supports context-sensitive lookups based on the `connectedPort` assigned. 
/// </summary>
public class SearchProvider : ScriptableObject, ISearchWindowProvider
{
    // The node + the screen position of the mouse at the time of selection
    public Action<NodeType, NodePort, Vector2> onCreateNode;
    internal NodePort connectedPort;

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
            var entry = new SearchTreeEntry(new GUIContent(node.Name));
            entry.level = 2;
            entry.userData = node;
            tree.Add(entry);
        }
        
        // TODO: Context sensitive based on `connectedPort`
        // TODO: Blacklisting certain nodes by some other context (e.g. two independent graph systems)
        
        return tree;
    }

    /// <summary>
    /// Notify listeners with the type of node selected and any other context information
    /// </summary>
    public bool OnSelectEntry(SearchTreeEntry entry, SearchWindowContext context)
    {
        onCreateNode?.Invoke(entry.userData as NodeType, connectedPort, context.screenMousePosition);
        return true;
    }
}
