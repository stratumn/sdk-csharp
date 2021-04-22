using Newtonsoft.Json;

namespace Stratumn.Sdk.Model.Trace
{
    /// <summary>
    /// Interface used as argument to create a new trace.
    /// User must provide the form id to use and the form data.
    /// </summary>
    public class NewTraceInput<TLinkData>
    {
        private string actionKey;

        private TLinkData data;

        private string groupLabel;

        public NewTraceInput(string actionKey, TLinkData data)
        {
            if (string.ReferenceEquals(actionKey, null))
            {
                throw new System.ArgumentException("actionKey cannot be null in NewTraceInput");
            }
            this.actionKey = actionKey;
            this.data = data;
        }

        public NewTraceInput(string actionKey, TLinkData data, string groupLabel)
        {
            if (string.ReferenceEquals(actionKey, null))
            {
                throw new System.ArgumentException("actionKey cannot be null in NewTraceInput");
            }
            this.actionKey = actionKey;
            this.data = data;
            this.groupLabel = groupLabel;
        }

        [JsonProperty(PropertyName = "formId")]
        public string FormId
        {
            get
            {
                return this.actionKey;
            }
            set
            {
                this.actionKey = value;
            }
        }

        [JsonProperty(PropertyName = "actionKey")]
        public string ActionKey
        {
            get
            {
                return this.actionKey;
            }
            set
            {
                this.actionKey = value;
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

        [JsonProperty(PropertyName = "groupLabel")]
        public string GroupLabel
        {
            get
            {
                return this.groupLabel;
            }
            set
            {
                this.groupLabel = value;
            }
        }
    }
}
