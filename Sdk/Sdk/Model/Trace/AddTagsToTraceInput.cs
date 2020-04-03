namespace Stratumn.Sdk.Model.Trace
{
    /// <summary>
    /// Interface used as argument to add tags to an existing trace.
    /// User must provide the trace id and the tags.
    /// </summary>
    public class AddTagsToTraceInput
    {
        private string traceId;
        private string[] tags;
        public AddTagsToTraceInput() : base()
        {
        }
        public AddTagsToTraceInput(string traceId, string[] tags) : base()
        {
            this.traceId = traceId;
            this.tags = tags;
        }
        public string TraceId
        {
            get
            {
                return traceId;
            }
            set
            {
                this.traceId = value;
            }
        }
        public string[] Tags
        {
            get
            {
                return tags;
            }
            set
            {
                this.tags = value;
            }
        }
    }
}
