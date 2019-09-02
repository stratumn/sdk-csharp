using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stratumn.Sdk.Model.Trace
{
    public class PushTransferInput<TLinkData> : ParentLink<TLinkData>
    {

        private string recipient;
        private TLinkData data;

        public PushTransferInput(string traceId, string recipient, TLinkData data, TraceLink<TLinkData> prevLink) : base(traceId, prevLink)
        {
            if (string.ReferenceEquals(recipient, null))
            {
                throw new System.ArgumentException("recipient cannot be null in PushTransferInput");
            }


            this.data = data;
            this.recipient = recipient;
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


        public string Recipient
        {
            get
            {
                return this.recipient;
            }
            set
            {
                this.recipient = value;
            }
        }



    }

}
