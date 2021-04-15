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

        private IList<string> tags;
        private SEARCH_TYPE searchType;

        public SearchTracesFilter() : base() { }

        // By default, search for any tags (for non breaking change)
        public SearchTracesFilter(IList<string> tags) : base()
        {
            this.tags = tags;
        }

        public Dictionary<String, object> GetFilters()
        {
            Dictionary<String, object> filters = new Dictionary<String, object>();
            Dictionary<String, object> searchFilter = new Dictionary<String, object>();

            if (SEARCH_TYPE.TAGS_CONTAINS == this.searchType)
            {
                searchFilter.Add("contains", this.Tags);
                filters.Add("tags", searchFilter);
            }
            else
            {
                // By default, search for any tags (for non breaking change)
                searchFilter.Add("overlaps", this.Tags);
                filters.Add("tags", searchFilter);
            }

            return filters;
        }

        public IList<string> Tags
        {
            get => tags;
            set => tags = value;
        }

        public SEARCH_TYPE SearchType
        {
            get => searchType;
            set => searchType = value;
        }
    }
}
