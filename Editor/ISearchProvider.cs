
using System.Collections.Generic;

namespace BlueGraph.Editor
{
    public class SearchResult
    {
        public string name;
        public IEnumerable<string> path;
        public IEnumerable<string> tags;
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
    }

    public interface ISearchProvider
    {
        IEnumerable<SearchResult> GetSearchResults(SearchFilter filter);
        AbstractNode Instantiate(SearchResult result);
    }
}
