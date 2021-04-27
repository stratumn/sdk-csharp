using System;

namespace Stratumn.Sdk.Model.Trace
{
    /// <summary>
    /// The state of trace is composed of:
    /// - the trace id
    /// - the link hash of the head of the trace
    /// - the date at which it was last updated
    /// - the person who last updated it
    /// - some abstract data validated against a predefined schema
    /// - the tags of the trace
    /// </summary>
    public class TraceState<TState, TLinkData>
    {
        private string traceId;
        private ITraceLink<TLinkData> headLink;
        private DateTime updatedAt;
        private Account updatedBy;
        private String updatedByGroupId;
        private TState data;
        private string[] tags;

        public TraceState(string traceId, ITraceLink<TLinkData> headLink, DateTime updatedAt, Account updatedBy, TState data, string[] tags, String updatedByGroupId)
        {
            if (string.ReferenceEquals(traceId, null))
            {
                throw new System.ArgumentException("traceId cannot be null in a TraceState object");
            }

            this.traceId = traceId;
            this.headLink = headLink;
            this.updatedAt = updatedAt;
            this.updatedBy = updatedBy;
            this.data = data;
            this.tags = tags;
            this.updatedByGroupId = updatedByGroupId;

        }

        public virtual string TraceId
        {
            get
            {
                return this.traceId;
            }
            set
            {
                this.traceId = value;
            }
        }

        public virtual ITraceLink<TLinkData> HeadLink
        {
            get
            {
                return this.headLink;
            }
            set
            {
                this.headLink = value;
            }
        }

        public virtual DateTime UpdatedAt
        {
            get
            {
                return this.updatedAt;
            }
            set
            {
                this.updatedAt = value;
            }
        }

        public virtual Account UpdatedBy
        {
            get
            {
                return this.updatedBy;
            }
            set
            {
                this.updatedBy = value;
            }
        }

        public virtual TState Data
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

        public virtual string[] Tags
        {
            get
            {
                return tags;
            }
            set
            {
                this.tags = value;
            }
        }

        public virtual string UpdatedByGroupId
        {
            get
            {
                return updatedByGroupId;
            }
            set
            {
                this.updatedByGroupId = value;
            }
        }

        public override string ToString()
        {
            return "TraceState [traceId=" + traceId + ", headLink=" + headLink + ", updatedAt=" + updatedAt + ", updatedBy=" + updatedBy + ", data=" + data + ", tags=" + tags.ToString() + ", updatedByGroupId=" + updatedByGroupId + "]";
        }
    }
}
