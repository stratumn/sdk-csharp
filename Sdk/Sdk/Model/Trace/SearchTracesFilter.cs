namespace Stratumn.Sdk.Model.Trace
{

    using System.Collections.Generic;
    /***
     *  The trace filter object used to search through all traces
     * of a workflow. 
     */
    public class SearchTracesFilter
    {
        private IList<string> tags;

        public SearchTracesFilter(IList<string> tags)
        {
            this.tags = tags;
        }

        public IList<string> Tags
        {
            get => tags;
            set => tags = value;
        }
    }
}
