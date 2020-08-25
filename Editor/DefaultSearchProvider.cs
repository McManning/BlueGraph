
using System;
using System.Collections.Generic;
using System.Linq;

namespace BlueGraph.Editor
{
    /// <summary>
    /// Default implementation of a SearchProvider for nodes. 
    /// 
    /// This provider provides all nodes found via NodeReflection.
    /// </summary>
    class DefaultSearchProvider : ISearchProvider
    {
        public IEnumerable<SearchResult> GetSearchResults(SearchFilter filter)
        {
            foreach (var entry in NodeReflection.GetNodeTypes())
            {
                var node = entry.Value;
                if (
                    IsCompatible(filter.sourcePort, node) && 
                    IsInSupportedTags(filter, node.tags)
                ) {
                    yield return new SearchResult
                    {
                        name = node.name,
                        path = node.path,
                        userData = node,
                    };
                }
            }
        }

        public Node Instantiate(SearchResult result)
        {
            NodeReflectionData data = result.userData as NodeReflectionData;
            return data.CreateInstance();
        }
        
        /// <summary>
        /// Returns true if the intersection between the tags and our allow
        /// list has more than one tag, OR if our allow list is empty.
        /// </summary>
        bool IsInSupportedTags(SearchFilter filter, IEnumerable<string> tags)
        {
            // If we have no include list, allow anything.
            if (filter.includeTags.Count() < 1) return true;
            
            // Otherwise - only allow if at least one tag intersects. 
            return filter.includeTags.Intersect(tags).Count() > 0;
        }

        bool IsCompatible(Port sourcePort, NodeReflectionData node)
        {
            if (sourcePort == null)
            {
                return true;
            }

            if (sourcePort.Direction == PortDirection.Input)
            {
                return node.HasOutputOfType(sourcePort.Type);
            }

            return node.HasInputOfType(sourcePort.Type);
        }
    }
}
