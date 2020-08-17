
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using System.Linq;

namespace BlueGraph.Editor
{
    public class SearchWindow : ScriptableObject, ISearchWindowProvider
    {
        public CanvasView target;
        public PortView sourcePort;

        /// <summary>
        /// If non-empty, only nodes with these tags may be included in search results.
        /// </summary>
        public List<string> includeTags = new List<string>();

        HashSet<ISearchProvider> m_Providers = new HashSet<ISearchProvider>();
        
        public void ClearTags()
        {
            includeTags.Clear();
        }

        public void ClearSearchProviders()
        {
            m_Providers.Clear();
        }

        public void AddSearchProvider(ISearchProvider provider)
        {
            m_Providers.Add(provider);
        }

        class SearchGroup
        {
            public SearchGroup(int depth)
            {
                this.depth = depth;
            }
            
            internal int depth;
            internal SortedDictionary<string, SearchGroup> subgroups = new SortedDictionary<string, SearchGroup>();
            internal List<SearchResult> results = new List<SearchResult>();

            internal void AddToTree(List<SearchTreeEntry> tree)
            {
                SearchTreeEntry entry;
                
                // Add subgroups
                foreach (var group in subgroups)
                {
                    entry = new SearchTreeGroupEntry(new GUIContent(group.Key))
                    {
                        level = depth
                    };

                    tree.Add(entry);
                    group.Value.AddToTree(tree);
                }

                // Add nodes
                foreach (var result in results)
                {
                    entry = new SearchTreeEntry(new GUIContent(result.name))
                    {
                        level = depth,
                        userData = result
                    };

                    tree.Add(entry);
                }
            }
        }

        IEnumerable<SearchResult> FilterSearchProviders(SearchFilter filter)
        {
            foreach (var provider in m_Providers)
            {
                foreach (var result in provider.GetSearchResults(filter))
                {
                    result.provider = provider;
                    yield return result;
                }
            }
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var filter = new SearchFilter
            {
                sourcePort = sourcePort?.target,
                includeTags = includeTags
            };

            // First item is the title of the window
            var tree = new List<SearchTreeEntry>();
            tree.Add(new SearchTreeGroupEntry(new GUIContent("Add Node"), 0));
            
            // Construct a tree of available nodes by module path
            var groups = new SearchGroup(1);
            
            // Aggregate search providers and get nodes matching the filter
            foreach (var result in FilterSearchProviders(filter))
            {
                var path = result.path;
                var group = groups;

                if (path != null)
                {
                    // If a path is defined, drill down into nested
                    // SearchGroup entries until we find the matching directory
                    foreach (var directory in path)
                    {
                        if (!group.subgroups.ContainsKey(directory))
                        {
                            group.subgroups.Add(directory, new SearchGroup(group.depth + 1));
                        }

                        group = group.subgroups[directory];
                    }
                }

                group.results.Add(result);
            }
            
            groups.AddToTree(tree);

            return tree;
        }

        public bool OnSelectEntry(SearchTreeEntry entry, SearchWindowContext context)
        {
            var result = entry.userData as SearchResult;
            var node = result.provider.Instantiate(result);

            target.AddNodeFromSearch(
                node,
                context.screenMousePosition, 
                sourcePort
            );

            return true;
        }
    }
}
