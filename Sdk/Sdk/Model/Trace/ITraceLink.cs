using System;
using Stratumn.Chainscript;

namespace Stratumn.Sdk.Model.Trace
{
    /// <summary>
    /// Interface extending a Chainscript Link
    /// with common trace methods.
    /// </summary>
    public interface ITraceLink<TLinkData> : ILink
    {
        TLinkData FormData();
        string TraceId();
        string WorkflowId();
        TraceLinkType Type();
        Account CreatedBy();
        DateTime CreatedAt();
        string Group();
        string Form();
        string LastForm();
        string[] Inputs();
        new TraceLinkMetaData Metadata();
    }
}
