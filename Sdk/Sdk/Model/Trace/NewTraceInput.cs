using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stratumn.Sdk.Model.Trace
{
    /// <summary>
    /// Interface used as argument to create a new trace.
    /// User must provide the form id to use and the form data.
    /// </summary>
    public class NewTraceInput<TLinkData>
    {

        private string formId;

        private TLinkData data;

        public NewTraceInput(string formId, TLinkData data)
        {
            if (string.ReferenceEquals(formId, null))
            {
                throw new System.ArgumentException("formId cannot be null in NewTraceInput");
            }
            this.formId = formId;
            this.data = data;
        }

        [JsonProperty(PropertyName = "formId")]
        public string FormId
        {
            get
            {
                return this.formId;
            }
            set
            {
                this.formId = value;
            }
        }


        [JsonProperty(PropertyName = "data")]
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
