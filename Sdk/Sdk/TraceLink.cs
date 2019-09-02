using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stratumn.Sdk
{
    using System;
    using Stratumn.Chainscript;
    using Stratumn.Sdk.Model.Trace;
    using Stratumn.Chainscript.utils;

    /// <summary>
    /// A TraceLink is an extension of a Chainscript Link
    /// that provides useful methods
    /// </summary>
    public class TraceLink<TLinkData> : Link, ITraceLink<TLinkData>
    {
        private TLinkData formData;

        public TraceLink(Link link, TLinkData formData) : base(link.ALink)
        {
            this.formData = formData;
        }


        public TLinkData FormData()
        {

            return formData != null ? formData : JsonHelper.ObjectToObject<TLinkData>(base.Data());
        }

        public string TraceId()
        {
            return base.MapId();
        }

        public string WorkflowId()
        {
            return base.Process().Name;
        }


        public TraceLinkType Type()
        {
            return (TraceLinkType)Enum.Parse(typeof(TraceLinkType), base.Process().State);
        }

        /// <summary>
        /// Get the metadata as TraceLinkMetaData
        /// </summary>
        /// <returns></returns>
        public TraceLinkMetaData Metadata()
        {

            TraceLinkMetaData traceLinkMd = JsonHelper.ObjectToObject< TraceLinkMetaData>(base.Metadata());
            return traceLinkMd;
        }

        public Account CreatedBy()
        {
            return new Account(this.Metadata().CreatedById);
        }

        public DateTime CreatedAt()
        {
            return this.Metadata().CreatedAt;
        }

        public  Account Owner()
        {
            return new Account(this.Metadata().OwnerId);
        }

        /// <summary>
        /// The id of the group under which the trace is. </summary>
        /// <exception cref="ChainscriptException"> </exception>
        /// <exception cref="Exception"> 
        /// 
        /// @returns the group id </exception>
        public  string Group()
        {
            return this.Metadata().GroupId;
        }
        /// <summary>
        /// The id of the form that was used to create the link. </summary>
        /// <exception cref="ChainscriptException"> </exception>
        /// <exception cref="Exception"> 
        /// 
        /// @returns the form id </exception>
        public string Form()
        {
            return this.Metadata().FormId;
        }
        /// <summary>
        /// The id of the form that was last used to create the link. </summary>
        /// <exception cref="ChainscriptException"> </exception>
        /// <exception cref="Exception"> 
        /// 
        /// @returns the last form id </exception>
        public string LastForm()
        {
            return this.Metadata().LastFormId;
        }
        /// <summary>
        /// The inputs of the link, used for transfer of ownership. </summary>
        /// <exception cref="ChainscriptException"> </exception>
        /// <exception cref="Exception"> 
        /// 
        /// @returns the inputs (array) </exception>
        public string[] Inputs()
        {
            return this.Metadata().Inputs;
        }


        /// <summary>
        /// Convert a plain object to a TraceLink. </summary>
        /// <param name="rawLink"> plain object. </param>
        public static TraceLink<TLinkData> FromObject<TLinkData>(string rawLink, TLinkData formData)
        {
            return new TraceLink<TLinkData>(Chainscript.Link.FromObject(rawLink), formData);
        }

    }

}
