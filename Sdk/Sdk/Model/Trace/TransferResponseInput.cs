namespace Stratumn.Sdk.Model.Trace
{
    public class TransferResponseInput<TLinkData> : ParentLink<TLinkData>
    {
        private TLinkData data;

        public TransferResponseInput(string traceId, TraceLink<TLinkData> prevLink) : base(traceId, prevLink)
        { 
        }

        public TransferResponseInput(string traceId, TLinkData data, TraceLink<TLinkData> prevLink) : base(traceId, prevLink)
        {

            this.data = data;
        }

        public virtual TLinkData Data
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
