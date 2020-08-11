
using System;
using System.Collections.Generic;

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
                if (IsCompatible(filter.sourcePort, node))
                {
                    yield return new SearchResult
                    {
                        name = node.name,
                        path = node.path,
                        tags = node.tags,
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
        
        bool IsCompatible(Port sourcePort, NodeReflectionData node)
        {
            if (sourcePort == null)
            {
                return true;
            }

            if (sourcePort.direction == PortDirection.Input)
            {
                return node.HasOutputOfType(sourcePort.type);
            }

            return node.HasInputOfType(sourcePort.type);
        }
    }
}
