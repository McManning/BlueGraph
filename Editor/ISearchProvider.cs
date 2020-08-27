using System.Collections.Generic;

namespace BlueGraph.Editor
{
    public interface ISearchProvider
    {
        IEnumerable<SearchResult> GetSearchResults(SearchFilter filter);

        Node Instantiate(SearchResult result);
    }

    public class SearchResult
    {
        public string Name { get; set; }

        public IEnumerable<string> Path { get; set; }

        public object UserData { get; set; }

        public ISearchProvider Provider { get; set; }
    }

    public class SearchFilter
    {
        /// <summary>
        /// If the user is dragging a port out to search for nodes
        /// that are compatible, this is that source port.
        /// </summary>
        public Port SourcePort { get; set; }

        /// <summary>
        /// List of tags in the Graph's [IncludeTags] attribute
        /// </summary>
        public IEnumerable<string> IncludeTags { get; set; }
    }
}
