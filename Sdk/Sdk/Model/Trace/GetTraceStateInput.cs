using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stratumn.Sdk.Model.Trace
{
    public class GetTraceStateInput
    {
        private string traceId;

        public GetTraceStateInput(string traceId)
        {
            if (string.ReferenceEquals(traceId, null))
            {
                throw new System.ArgumentException("traceId cannot be null in GetTraceStateInput");
            }
            this.traceId = traceId;
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
    }
}
