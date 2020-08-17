
using System.Collections.Generic;

namespace BlueGraph.Editor
{
    public class SearchResult
    {
        public string name;
        public IEnumerable<string> path;
        public object userData;

        public ISearchProvider provider;
    }

    public class SearchFilter
    {
        /// <summary>
        /// If the user is dragging a port out to search for nodes
        /// that are compatible, this is that source port.
        /// </summary>
        public Port sourcePort;

        /// <summary>
        /// List of tags in the Graph's [IncludeTags] attribute
        /// </summary>
        public IEnumerable<string> includeTags;
    }

    public interface ISearchProvider
    {
        IEnumerable<SearchResult> GetSearchResults(SearchFilter filter);
        Node Instantiate(SearchResult result);
    }
}
