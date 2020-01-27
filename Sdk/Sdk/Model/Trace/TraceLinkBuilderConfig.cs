using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stratumn.Sdk.Model.Trace
{
    /// <summary>
    /// The configuration interface for a new TraceLinkBuilder.
    /// </summary>
    public class TraceLinkBuilderConfig<TLinkData>
    {

        private string workflowId;
        private string configId;
        private ITraceLink<TLinkData> parentLink;

        public TraceLinkBuilderConfig()
        {

        }
        public TraceLinkBuilderConfig(string workflowId, ITraceLink<TLinkData> parentLink)
        {
            this.workflowId = workflowId;
            this.parentLink = parentLink;
        }

        public string WorkflowId
        {
            get
            {
                return workflowId;
            }
            set
            {
                this.workflowId = value;
            }
        }
        public string ConfigId
        {
            get
            {
                return configId;
            }
            set
            {
                this.configId = value;
            }
        }
        public ITraceLink<TLinkData> ParentLink
        {
            get
            {
                return parentLink;
            }
            set
            {
                this.parentLink = value;
            }
        }

    }

}
