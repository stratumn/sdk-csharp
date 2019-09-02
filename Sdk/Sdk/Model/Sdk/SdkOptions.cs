using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stratumn.Sdk.Model.Client;

namespace Stratumn.Sdk.Model.Sdk
{
    public class SdkOptions : ClientOptions
    {
        public SdkOptions(string workflowId, Secret secret) : base(null, secret)
        {
            this.WorkflowId = workflowId;
        }

        public string WorkflowId { get; set; }

    }
}
