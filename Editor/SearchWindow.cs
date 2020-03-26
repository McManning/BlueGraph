
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;

namespace BlueGraph.Editor
{
    public class SearchWindow : ScriptableObject, ISearchWindowProvider
    {
        public CanvasView target;
        public PortView sourcePort;

        /// <summary>
        /// Whitelist of node modules to include in search results.
        /// All submodules will be included (e.g. "Foo" will include "Foo/Bar", "Foo/Fizz/Buzz", etc).
        /// </summary>
        List<string[]> m_Modules = new List<string[]>();

        HashSet<ISearchProvider> m_Providers = new HashSet<ISearchProvider>();
        
        public void ClearModules()
        {
            m_Modules.Clear();
        }

        public void AddModule(string module)
        {
            m_Modules.Add(module.Split('/'));
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
            List<SearchResult> results = new List<SearchResult>();
            foreach (var provider in m_Providers)
            {
                foreach (var result in provider.GetSearchResults(filter))
                {
                    result.provider = provider;
                    results.Add(result);
                }
            }

            return results;
        }
        
        /// <summary>
        /// Returns true if the input path is prefixed by one or more registered module paths.
        /// </summary>
        bool IsInSupportedModules(string[] path)
        {
            // Everything in global namespace is allowed.
            if (path == null) return true;
    
            foreach (var module in m_Modules)
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

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            SearchFilter filter = new SearchFilter
            {
                sourcePort = sourcePort?.target
            };

            // First item is the title of the window
            List<SearchTreeEntry> tree = new List<SearchTreeEntry>();
            tree.Add(new SearchTreeGroupEntry(new GUIContent("Add Node"), 0));
            
            // Construct a tree of available nodes by module path
            SearchGroup groups = new SearchGroup(1);
            
            // Aggregate search providers and get nodes matching the filter
            foreach (SearchResult result in FilterSearchProviders(filter))
            {
                var path = result.path;
                SearchGroup group = groups;
                if (path != null && IsInSupportedModules(path))
                {
                    // If a module path is defined, drill down into nested
                    // SearchGroup entries until we find the matching directory
                    for (int i = 0; i < path.Length; i++)
                    {
                        if (!group.subgroups.ContainsKey(path[i]))
                        {
                            group.subgroups.Add(path[i], new SearchGroup(group.depth + 1));
                        }

                        group = group.subgroups[path[i]];
                    }
                }

                group.results.Add(result);
            }
            
            groups.AddToTree(tree);

            return tree;
        }

        public bool OnSelectEntry(SearchTreeEntry entry, SearchWindowContext context)
        {
            SearchResult result = entry.userData as SearchResult;
            AbstractNode node = result.provider.Instantiate(result);

            target.AddNodeFromSearch(
                node,
                context.screenMousePosition, 
                sourcePort
            );

            return true;
        }
    }
}
