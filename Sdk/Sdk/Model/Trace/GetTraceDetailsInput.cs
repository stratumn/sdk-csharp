using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stratumn.Sdk.Model.Trace
{
    public class GetTraceDetailsInput : PaginationInfo
    {

        private string traceId;

        public GetTraceDetailsInput(string traceId, int? first, string after, int? last, string before) : base(first, after, last, before)
        {
            if (string.ReferenceEquals(traceId, null))
            {
                throw new System.ArgumentException("traceId cannot be null in GetTraceDetailsInput");
            }

            this.traceId = traceId;
        }

        [JsonProperty(PropertyName = "traceId")]
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
