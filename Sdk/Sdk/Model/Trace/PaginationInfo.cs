using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stratumn.Sdk.Model.Trace
{
    public class PaginationInfo
    {
     

        [JsonProperty(PropertyName = "first")]
        public int? First { get; set; }

        [JsonProperty(PropertyName = "after")]
        public string After { get; set; }

        [JsonProperty(PropertyName = "last")]
        public int? Last { get; set; }

        [JsonProperty(PropertyName = "before")]
        public string Before { get; set; }

        public PaginationInfo()
        {
        }
        public PaginationInfo(int? first, string after, int? last, string before)
        {
            if (first != null)
            {
                this.First = first;
            }
            if (!string.ReferenceEquals(after, null))
            {
                this.After = after;
            }
            if (last != null)
            {
                this.Last = last;
            }
            if (!string.ReferenceEquals(before, null))
            {
                this.Before = before;
            }

        }


        public void SetForward(int first, string after)
        {
            this.First = first;
            this.After = after;
        }

        public void SetBackward(int last, string before)
        {
            this.Last = last;
            this.Before = before;
        }


        
      

    }

}
