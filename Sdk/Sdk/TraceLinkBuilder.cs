namespace Stratumn.Sdk
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Stratumn.Chainscript;
    using Stratumn.Sdk.Model.Trace;
    using Utils;
    using Stratumn.Chainscript.utils;

    /// <summary>
    /// TraceLinkBuilder makes it easy to create links that are compatible
    /// with Trace.
    /// It provides valid default values for required fields and allows the user
    /// to set fields to valid values.
    /// </summary>
    /// <typeparam name="TLinkData"></typeparam>
    public class TraceLinkBuilder<TLinkData> : LinkBuilder
    {
        /// <summary>
        /// Defines the metadata
        /// </summary>
        private TraceLinkMetaData metadata;

        /// <summary>
        /// Defines the parentLink
        /// </summary>
        private ITraceLink<TLinkData> parentLink;

        /// <summary>
        /// Defines the formData
        /// </summary>
        private TLinkData formData;

        /// <summary>
        /// Initializes a new instance of the <see cref="TraceLinkBuilder{TLinkData}"/> class.
        /// </summary>
        /// <param name="cfg"> the config to instantiate the builder </param>
        public TraceLinkBuilder(TraceLinkBuilderConfig<TLinkData> cfg) : base(
                          cfg.WorkflowId, (cfg.ParentLink != null ? cfg.ParentLink.TraceId() : Guid.NewGuid().ToString()))
        {

            // trace id is either retrieved from parent link when it is provided
            // or set to a new uuid.

            // set the parent link
            this.parentLink = (TraceLink<TLinkData>)cfg.ParentLink;

            // degree is always 1
            base.WithDegree(1);
            // set priority to 1 by default
            // may be overriden if parent link was provided
            base.WithPriority(1);

            // set the created at timestamp 
            this.metadata = new TraceLinkMetaData();
            this.metadata.CreatedAt = DateTime.Now;

            // if parent link was provided set the parent hash and priority
            if (this.parentLink != null)
            {
                base.WithPriority(this.parentLink.Priority() + 1).WithParent(this.parentLink.Hash());
            }
        }

        /// <summary>
        /// Gets the ParentLink
        /// Helper method to get the parent link. Will throw if no parent link was
        /// provided.
        /// </summary>
        public TraceLink<TLinkData> ParentLink
        {
            get
            {
                if (this.parentLink == null)
                {
                    throw new TraceSdkException("Parent link must be provided");
                }
                return (TraceLink<TLinkData>)this.parentLink;
            }
        }

        /// <summary>
        /// Set the data field to the hash of the object argument.
        /// </summary>
        /// <param name="obj"> the optional object to be hashed </param>
        /// <returns>The <see cref="TraceLinkBuilder{TLinkData}"/></returns>
        public TraceLinkBuilder<TLinkData> WithHashedData(TLinkData obj)
        {
            if (obj != null)
            {
                string algo = "sha256";

                string hash = Convert.ToBase64String(
                                        CryptoUtils.Sha256(
                                                Encoding.UTF8.GetBytes(
                                                    CanonicalJson.Canonicalizer.Stringify(obj)
                                                    )
                                                )
                                        );

               
                IDictionary<string, object> data = new Dictionary<string, object>();
                data["algo"] = algo;
                data["hash"] = hash;
                this.WithData(data);
                this.formData = obj;
            }
            return this;
        }

        /// <summary>
        /// Helper method used to configure a link for an attestation. User must still
        /// set owner, group and createdBy separately.
        /// </summary>
        /// <param name="formId"> the form id used for the attestation </param>
        /// <param name="action"> the name of the action associated with this form </param>
        /// <param name="data">   the data of the attestation </param>
        /// <returns>The <see cref="TraceLinkBuilder{TLinkData}"/></returns>
        public TraceLinkBuilder<TLinkData> ForAttestation(string formId, string action, TLinkData data)
        {
            string actionStr = !string.ReferenceEquals(action, null) ? action : "Attestation"; //TraceActionType.ATTESTATION.toString();
            string typeStr = TraceLinkType.OWNED.ToString();
            this.WithHashedData(data)
                    .WithAction(actionStr)
                        .WithProcessState(typeStr);
            this.metadata.FormId = formId;
            return this;
        }

        /// <summary>
        /// Helper method used for transfer of ownership requests (push and pull). Note
        /// that owner and group are calculated from parent link. Parent link must have
        /// been provided!
        /// </summary>
        /// <param name="to">     the group to which the transfer is made for </param>
        /// <param name="action"> the action (_PUSH_OWNERSHIP_ or _PULL_OWNERSHIP_) </param>
        /// <param name="type">   the type (PUSHING OR PULLING) </param>
        /// <param name="data">   the optional data </param>
        /// <returns>The <see cref="TraceLinkBuilder{TLinkData}"/></returns>
        public TraceLinkBuilder<TLinkData> ForTransferRequest(string to, TraceActionType action, TraceLinkType type, TLinkData data)
        {
            TraceLink<TLinkData> parent = this.ParentLink;
            this.WithOwner(parent.Owner().GetAccount())
                    .WithGroup(parent.Group())
                        .WithHashedData(data)
                            .WithAction(action.ToString())
                                 .WithProcessState(type.ToString());

            this.metadata.Inputs = new string[] { to };
            this.metadata.LastFormId = parent.Form() != null ? parent.Form() : parent.LastForm();
            return this;
        }

        /// <summary>
        /// Helper method used for pushing ownership to another group.
        /// </summary>
        /// <param name="to">   the group to which the trace is pushed to </param>
        /// <param name="data"> the optional data </param>
        /// <returns>The <see cref="TraceLinkBuilder{TLinkData}"/></returns>
        public TraceLinkBuilder<TLinkData> ForPushTransfer(string to, TLinkData data)
        {
            return this.ForTransferRequest(to, TraceActionType.PUSH_OWNERSHIP, TraceLinkType.PUSHING, data);
        }

        /// <summary>
        /// Helper method used for pulling ownership from another group.
        /// </summary>
        /// <param name="to">   the group to which the trace is pulled to </param>
        /// <param name="data"> the optional data </param>
        /// <returns>The <see cref="TraceLinkBuilder{TLinkData}"/></returns>
        public TraceLinkBuilder<TLinkData> ForPullTransfer(string to, TLinkData data)
        {
            return this.ForTransferRequest(to, TraceActionType.PULL_OWNERSHIP, TraceLinkType.PULLING, data);
        }

        /// <summary>
        /// Helper method used to cancel a transfer request. Note that owner and group
        /// are calculated from parent link. Parent link must have been provided!
        /// </summary>
        /// <param name="data"> the optional data </param>
        /// <returns>The <see cref="TraceLinkBuilder{TLinkData}"/></returns>
        public TraceLinkBuilder<TLinkData> ForCancelTransfer(TLinkData data)
        {
            TraceLink<TLinkData> parent = this.ParentLink;
            string action = TraceActionType.CANCEL_TRANSFER.ToString();
            string type = TraceLinkType.OWNED.ToString();
            this.WithOwner(parent.Owner().GetAccount())
                    .WithGroup(parent.Group())
                        .WithHashedData(data)
                            .WithAction(action)
                                .WithProcessState(type);
            return this;
        }

        /// <summary>
        /// Helper method used to reject a transfer request. Note that owner and group
        /// are calculated from parent link. Parent link must have been provided!
        /// </summary>
        /// <param name="data"> the optional data </param>
        /// <returns>The <see cref="TraceLinkBuilder{TLinkData}"/></returns>
        public TraceLinkBuilder<TLinkData> ForRejectTransfer(TLinkData data)
        {
            TraceLink<TLinkData> parent = this.ParentLink;
            string action = TraceActionType.REJECT_TRANSFER.ToString();
            string type = TraceLinkType.OWNED.ToString();

            this.WithOwner(parent.Owner().GetAccount())
                    .WithGroup(parent.Group())
                        .WithHashedData(data)
                            .WithAction(action)
                                .WithProcessState(type);
            return this;
        }

        /// <summary>
        /// Helper method used to accept a transfer request. Parent link must have been
        /// provided! User must still set owner, group and createdBy separately.
        /// </summary>
        /// <param name="data"> the optional data </param>
        /// <returns>The <see cref="TraceLinkBuilder{TLinkData}"/></returns>
        public TraceLinkBuilder<TLinkData> ForAcceptTransfer(TLinkData data)
        {
            // call parent link to assert it was set
            if (this.ParentLink != null)
            {

                string action = TraceActionType.ACCEPT_TRANSFER.ToString();
                string type = TraceLinkType.OWNED.ToString();

                this.WithHashedData(data).WithAction(action).WithProcessState(type);

            }
            return this;
        }

        /// <summary>
        /// To set the metadata ownerId.
        /// </summary>
        /// <param name="ownerId"> the owner id </param>
        /// <returns>The <see cref="TraceLinkBuilder{TLinkData}"/></returns>
        public virtual TraceLinkBuilder<TLinkData> WithOwner(string ownerId)
        {
            this.metadata.OwnerId = ownerId;
            return this;
        }

        /// <summary>
        /// To set the metadata groupId.
        /// </summary>
        /// <param name="groupId"> the group id </param>
        /// <returns>The <see cref="TraceLinkBuilder{TLinkData}"/></returns>
        public virtual TraceLinkBuilder<TLinkData> WithGroup(string groupId)
        {
            this.metadata.GroupId = groupId;
            return this;
        }

        /// <summary>
        /// To set the metadata createdById.
        /// </summary>
        /// <param name="userId">The userId<see cref="string"/></param>
        /// <returns>The <see cref="TraceLinkBuilder{TLinkData}"/></returns>
        public virtual TraceLinkBuilder<TLinkData> WithCreatedBy(string userId)
        {
            this.metadata.CreatedById = userId;
            return this;
        }

        /// <summary>
        /// The Build
        /// </summary>
        /// <returns>The <see cref="TraceLink{TLinkData}"/></returns>
        public virtual TraceLink<TLinkData> Build()
        {
            base.WithMetadata(this.metadata);
            Link link = base.Build();
            return new TraceLink<TLinkData>(link, JsonHelper.ObjectToObject<TLinkData>(this.formData));
        }
    }
}
