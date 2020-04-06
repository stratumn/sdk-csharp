namespace Stratumn.Sdk.Model.Trace
{
    /// <summary>
    ///*
    /// Class to hold the traceId or prevLink used to identify the previous link. </summary>
    /// @param <TLinkData> </param>
    public class ParentLink<TLinkData>
    {
        private string traceId;
        private TraceLink<TLinkData> prevLink;

        public ParentLink(string traceId, TraceLink<TLinkData> prevLink)
        {
            if (string.ReferenceEquals(traceId, null) && prevLink == null)
            {
                throw new System.ArgumentException("TraceId and PrevLink cannot be both null");
            }
            this.traceId = traceId;
            this.prevLink = prevLink;
        }

        public string TraceId
        {
            get
            {
                return this.traceId;
            }
            set
            {
                this.traceId = value;
            }
        }

        public TraceLink<TLinkData> PrevLink
        {
            get
            {
                return this.prevLink;
            }
            set
            {
                this.prevLink = value;
            }
        }
    }
}
