using StratumSdk.Model.Clients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StratumSdk
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
