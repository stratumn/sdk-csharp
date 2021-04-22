using Stratumn.Sdk.Model.Client;

namespace Stratumn.Sdk.Model.Sdk
{
    public class SdkOptions : ClientOptions
    {

        public SdkOptions(string workflowId, Secret secret) : base(null, secret)
        {
            this.WorkflowId = workflowId;
        }

        public SdkOptions(string workflowId, Secret secret, string groupLabel) : base(null, secret)
        {
            this.WorkflowId = workflowId;
            this.GroupLabel = groupLabel;
        }

        public string WorkflowId { get; set; }

        public string GroupLabel { get; set; }
    }
}
