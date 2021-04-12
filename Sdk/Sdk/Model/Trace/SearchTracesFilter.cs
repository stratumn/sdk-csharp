namespace Stratumn.Sdk.Model.Trace
{

    using System;
    using System.Collections.Generic;
    /***
     *  The trace filter object used to search through all traces
     * of a workflow. 
     */
    public class SearchTracesFilter
    {

        public enum SEARCH_TYPE
        {
            TAGS_CONTAINS, TAGS_OVERLAPS
        }

        private Dictionary<String, object> filters;

        // By default, search for any tags (for non breaking change)
        public SearchTracesFilter(IList<string> tags) : this(tags, SEARCH_TYPE.TAGS_OVERLAPS) { }

        public SearchTracesFilter(IList<string> tags, SEARCH_TYPE searchType)
        {

            this.filters = new Dictionary<String, object>();
            Dictionary<String, object> searchFilter = new Dictionary<String, object>();

            switch (searchType)
            {
                case SEARCH_TYPE.TAGS_CONTAINS:
                    // search for all tags
                    searchFilter.Add("contains", tags);
                    this.filters.Add("tags", searchFilter);
                    break;
                case SEARCH_TYPE.TAGS_OVERLAPS:
                    searchFilter.Add("overlaps", tags);
                    this.filters.Add("tags", searchFilter);
                    break;
                default:
                    // By default, search for any tags (for non breaking change)
                    searchFilter.Add("overlaps", tags);
                    this.filters.Add("tags", searchFilter);
                    break;
            }
        }

        public Dictionary<String, object> Filters
        {
            get => filters;
            set => filters = value;
        }
    }
}
