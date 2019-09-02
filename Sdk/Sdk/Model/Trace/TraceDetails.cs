using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stratumn.Sdk.Model.Trace
{
    using System.Collections.Generic;

    public class TraceDetails<TLinkData> : PaginationResults
    {

        public IList<TraceLink<TLinkData>> links;

        public TraceDetails()
        {
        }

        public TraceDetails(IList<TraceLink<TLinkData>> links, int totalCount, Info info) : base(totalCount, info)
        {
            this.links = links;
        }

        public IList<TraceLink<TLinkData>> Links
        {
            get
            {
                return this.links;
            }
            set
            {
                this.links = value;
            }
        }


    }

}
