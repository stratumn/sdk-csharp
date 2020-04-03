namespace Stratumn.Sdk.Model.Trace
{
    public class PullTransferInput<TLinkData> : ParentLink<TLinkData>
    {
        private TLinkData data;

       public PullTransferInput(string traceId, TLinkData data, TraceLink<TLinkData> prevLink) : base(traceId, prevLink)
        {
            this.data = data;
        }

        public TLinkData Data
        {
            get
            {
                return this.data;
            }
            set
            {
                this.data = value;
            }
        }
    }
}
