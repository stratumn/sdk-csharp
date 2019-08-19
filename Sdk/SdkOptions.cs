using StratumSdk.Model.Clients;

namespace stratumn.sdk
{
    public class SdkOptions : ClientOptions
    {
        public string WorkflowId { get; set; }


        public SdkOptions(string workflowId, Secret secret) : base(new Endpoints(), secret)
        {
            this.WorkflowId = workflowId;
        }

    }
}
