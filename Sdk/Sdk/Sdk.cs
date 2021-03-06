﻿using GraphQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Crypto.Parameters;

using Stratumn.Chainscript.Proto;
using Stratumn.Chainscript.utils;
using Stratumn.Sdk.Model.Client;
using Stratumn.Sdk.Model.Sdk;
using Stratumn.Sdk.Model.Trace;
using Stratumn.Sdk.Model.Misc;
using Stratumn.Sdk.Model.File;
using FileInfo = Stratumn.Sdk.Model.File.FileInfo;
using static Stratumn.Sdk.Client;
using Utils;

namespace Stratumn.Sdk
{
    /// <summary>
    /// Defines the <see cref="Sdk{TState}" />
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    public class Sdk<TState> : ISdk<TState>
    {
        /// <summary>
        /// Defines the config
        /// </summary>
        private SdkConfig config;

        /// <summary>
        /// Defines the opts
        /// </summary>
        private SdkOptions opts;

        /// <summary>
        /// Defines the client
        /// </summary>
        private Client client;

        private const String ERROR_CONFIG_DEPRECATED = "link config deprecated";

        /// <summary>
        /// Initializes a new instance of the <see cref="Sdk{TState}"/> class.
        /// </summary>
        /// <param name="opts">The opts<see cref="SdkOptions"/></param>
        public Sdk(SdkOptions opts)
        {
            this.opts = opts;
            JsonHelper.RegisterConverter(new IdentifiableConverter());
            this.client = new Client(opts);

        }

        /// <summary>
        /// Retrieves the Sdk config for the given workflow. If the config has not yet
        /// been comAdded, the Sdk will run a GraphQL query to retrieve the relevant info
        /// and will generate the config.
        /// </summary>
        /// <returns>The <see cref="Task{SdkConfig}"/></returns>
        public async Task<SdkConfig> GetConfigAsync(bool forceUpdate = false)
        {
            // update the config if doesn't exist or force
            if (this.config == null || forceUpdate)
            {
                string workflowId = this.opts.WorkflowId;
                String query = GraphQL.QUERY_CONFIG;

                Dictionary<string, object> variables = new Dictionary<string, object>() { { "workflowId", workflowId } };

                GraphQLResponse<dynamic> jsonResponse = await this.client.GraphqlAsync(query, variables, null, null);

                var jsonData = jsonResponse.Data;

                // extract relevant info from the response
                var workflow = jsonData.workflow;
                if (workflow == null || workflow.groups == null)
                {
                    throw new Exception("Cannot find workflow " + workflowId);
                }
                var groups = jsonData.workflow.groups;
                String configId = jsonData.workflow.config.id;
                var jsonAccount = jsonData.account;

                var user = jsonAccount.user;
                var bot = jsonAccount.bot;

                String accountId = jsonAccount.accountId;

                IList<string> myAccounts = new List<string>();

                // get all the account ids I am a member of
                if (user != null)
                {
                    foreach (var mNode in user.memberOf?.nodes)
                    {
                        myAccounts.Add((String)mNode.accountId);
                    }
                }
                else if (bot != null)
                {
                    foreach (var mNode in bot.teams.nodes)
                    {
                        myAccounts.Add((String)mNode.accountId);
                    }
                }

                IList<Object> myGroups = new List<Object>();
                Dictionary<string, string> groupLabelToIdMap = new Dictionary<string, string>();
                // get all the groups that are owned by one of my accounts
                foreach (var _group in groups.nodes)
                {
                    String groupLabel = (String)_group.label;
                    String groupId = (String)_group.groupId;
                    foreach (var member in _group.members.nodes)
                    {
                        if (myAccounts.Contains((String)member.accountId))
                        {
                            myGroups.Add((Object)_group);
                            groupLabelToIdMap.Add(groupLabel, groupId);
                        }
                    }
                }

                // // there must be at least one group!
                if (myGroups.Count == 0)
                {
                    Console.Write("Groups", groups.nodes);
                    throw new Exception("No group to choose from.");
                }

                Ed25519PrivateKeyParameters signingPrivateKey;

                if (Secret.IsPrivateKeySecret(opts.Secret))
                {
                    // if the secret is a PrivateKeySecret, use it!
                    String privateKey = ((PrivateKeySecret)opts.Secret).PrivateKey;
                    signingPrivateKey = CryptoUtils.DecodeEd25519PrivateKey(privateKey);
                }
                else
                {
                    var signingKey = jsonAccount.signingKey;
                    var privateKey = signingKey.privateKey;
                    Boolean passwordProtected = (Boolean)privateKey.passwordProtected;
                    String decrypted = (String)privateKey.decrypted;
                    if (!passwordProtected)
                        // otherwise use the key from the response
                        // if it's not password protected!
                        signingPrivateKey = CryptoUtils.DecodeEd25519PrivateKey(decrypted);
                    else
                        throw new Exception("Cannot get signing private key");
                }

                this.config = new SdkConfig(workflowId, configId, accountId, groupLabelToIdMap, signingPrivateKey);
            }

            // sets the group id in any case
            if (null != this.opts.GroupLabel)
            {
                this.config.GroupLabel = this.opts.GroupLabel;
            }

            // return the new config
            return this.config;
        }


        public Sdk<TState> withGroupLabel(String groupLabel)
        {
            this.opts.GroupLabel = groupLabel;
            return this;
        }

        /// <summary>
        /// Creates a new Link from the given builder, signs it and executes the GraphQL
        /// mutation.
        /// </summary>
        /// <typeparam name="TLinkData"></typeparam>
        /// <param name="linkBuilder">The linkBuilder<see cref="TraceLinkBuilder{TLinkData}"/></param>
        /// <param name="firstTry">if this is not the first try, do not retry</param>
        /// <returns>The <see cref="Task{TraceState{TState, TLinkData}}"/></returns>
        private async Task<TraceState<TState, TLinkData>> CreateLinkAsync<TLinkData>(TraceLinkBuilder<TLinkData> linkBuilder, bool firstTry = true)
        {
            // extract signing key from config
            SdkConfig sdkConfig = await GetConfigAsync();

            Ed25519PrivateKeyParameters signingPrivateKey = sdkConfig.SigningPrivateKey;

            // build the link
            TraceLink<TLinkData> link = linkBuilder.Build();

            // sign the link
            link.Sign(signingPrivateKey.GetEncoded(), "[version,data,meta]");



            string linkObjJson = JsonHelper.ToJson(link.ALink);

            Dictionary<string, object> linkObj = JsonHelper.ObjectToMap(link.GetLink());


            Dictionary<string, object> dataObj = JsonHelper.ObjectToMap(((TraceLink<TLinkData>)link).FormData());

            Dictionary<string, object> variables = new Dictionary<string, object>
            {
                ["link"] = linkObj,
                ["data"] = dataObj
            };

            try
            {
                // execute graphql query 
                GraphQLResponse<dynamic> jsonResponse = await this.client.GraphqlAsync(GraphQL.MUTATION_CREATELINK, variables, null, null);
                var trace = jsonResponse.Data.createLink.trace;

                return this.MakeTraceState<TLinkData>(trace);
            }
            catch (TraceSdkException e)
            {
                if (firstTry && e.Message == ERROR_CONFIG_DEPRECATED)
                {
                    var cfg = await this.GetConfigAsync(true);
                    linkBuilder.WithConfigId(cfg.ConfigId);
                    link.GetLink().Signatures.Clear();
                    return await this.CreateLinkAsync(linkBuilder, false);
                }

                throw e;
            }

        }

        /// <summary>
        /// Given a trace id or a previous link return the previous link.
        /// </summary>
        /// <typeparam name="TLinkData"></typeparam>
        /// <param name="input">       .traceId the id of the trace </param>
        /// <returns>The <see cref="Task{TraceLink{TLinkData}}"/></returns>
        public async Task<TraceLink<TLinkData>> GetHeadLinkAsync<TLinkData>(ParentLink<TLinkData> input)
        {
            TraceLink<TLinkData> headLink = input.PrevLink;
            // if prevLink was not provided  
            if (headLink == null && input.TraceId != null)
            {
                Dictionary<String, object> variables = new Dictionary<String, object>
                {
                    { "traceId", input.TraceId }
                };
                string query = GraphQL.QUERY_GETHEADLINK;
                // execute graphql query
                GraphQLResponse<dynamic> jsonResponse = await this.client.GraphqlAsync(query, variables, null, null);
                var trace = jsonResponse.Data.trace;

                string raw = trace.head.raw.ToString();

                TLinkData data = JsonHelper.ObjectToObject<TLinkData>(trace.head.data);

                // convert the raw response to a link object
                headLink = new TraceLink<TLinkData>(Stratumn.Chainscript.Link.FromObject(raw), data);

            }
            if (headLink != null)
            {
                return headLink;
            }
            else
            {
                throw new TraceSdkException("Previous link or trace Id must be provided");
            }
        }

        /// <summary>
        /// The LoginAsync
        /// </summary>
        /// <returns>The <see cref="Task{string}"/></returns>
        public async Task<string> LoginAsync()
        {
            Client client = new Client(this.opts);
            try
            {
                return await client.Login();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// Get the traces in a given stage (INCOMING, OUTGOING, BACKLOG, ATTESTATION)T
        /// When stageType=ATTESTATION, you must also provide the form id to identify the
        /// stage. If no stage correspond to the stageType x actionKey, it will throw. If
        /// more than one stage is found it will also throw.
        ///
        /// @param stageType      the stage type
        /// @param paginationInfo the pagination info
        /// @param actionKey (optional) the action key in case of ATTESTATION
        /// @return the traces in a given stage
        /// @throws Error
        /// @throws Exception
        ////
        /// </summary>
        /// <typeparam name="TLinkData"></typeparam>
        /// <param name="stageType">The stageType<see cref="TraceStageType"/></param>
        /// <param name="paginationInfo">The paginationInfo<see cref="PaginationInfo"/></param>
        /// <param name="actionKey">The action key<see cref="String"/></param>
        /// <returns>The <see cref="Task{TracesState{TState, TLinkData}}"/></returns>
        public async Task<TracesState<TState, TLinkData>> GetTracesInStageAsync<TLinkData>(TraceStageType stageType, PaginationInfo paginationInfo,
                String actionKey)
        {

            // actionKey can only be set in ATTESTATION case
            if (stageType == TraceStageType.ATTESTATION && actionKey == null)
            {
                throw new Exception("You must and can only provide actionKey when stageType is ATTESTATION");
            }
            // extract info from config
            SdkConfig sdkConfig = await this.GetConfigAsync();

            String groupId = sdkConfig.GetGroupId();

            // create variables
            Dictionary<String, object> variables = new Dictionary<String, object>
            {
                { "groupId", groupId },
                { "stageType", stageType.ToString() },
            };
            if (actionKey != null)
            {
                variables.Add("actionKey", actionKey);
            }

            Dictionary<String, object> variablesPaginationInfo = JsonHelper.ObjectToMap(paginationInfo);

            variablesPaginationInfo.ToList().ForEach(x => variables.Add(x.Key, x.Value));


            // execute the graphql query
            string query = GraphQL.QUERY_GETTRACESINSTAGE;
            GraphQLResponse<dynamic> jsonResponse = await this.client.GraphqlAsync(query, variables, null, null);
            var jsonData = jsonResponse.Data;


            // extract relevant info from the response
            var stages = jsonData.group.stages.nodes;

            // there must be exactly one stage
            if (stages.Count == 1)
            {
                var stage = stages[0];

                var trace = stage.traces;
                // extract traces response and pagination 
                var info = trace.info;
                int totalCount = trace.totalCount;

                List<TraceState<TState, TLinkData>> traces = new List<TraceState<TState, TLinkData>>();

                // get all the groups that are owned by one of my accounts
                var nodes = trace.nodes;
                foreach (var node in nodes)
                {
                    traces.Add(this.MakeTraceState<TLinkData>((JObject)node));
                }

                TracesState<TState, TLinkData> tracesList = new TracesState<TState, TLinkData>()
                {
                    Traces = traces,
                    TotalCount = totalCount,
                    Info = nodes.Count >= 1 ? info.ToObject<Info>() : null
                };
                return tracesList;
            }

            // comAdde detail for error
            String stageDetail = stageType.ToString() + actionKey ?? "";

            // throw if no stages were found if
            if (stages.size() == 0)
            {
                throw new Exception("No " + stageDetail + " stage");
            }
            // throw if multiple stages were found throw new
            throw new Exception("Multiple " + stageDetail + " stages");
        }

        /// <summary>
        /// Extract, upload and replace all file wrappers in a link data object.
        /// </summary>
        /// <typeparam name="TLinkData"></typeparam>
        /// <param name="data"> the link data that contains file wrappers to upload </param>
        public async Task UploadFilesInLinkData<TLinkData>(TLinkData data)
        {
            Dictionary<string, Property<FileWrapper>> fileWrapperMap = Helpers.ExtractFileWrappers(data);
            if (fileWrapperMap.Count == 0) return;


            List<FileWrapper> fileList = new List<FileWrapper>(fileWrapperMap.Count);

            foreach (Property<FileWrapper> fileProperty in fileWrapperMap.Values)
                fileList.Add(fileProperty.Value);

            MediaRecord[] mediaRecords = await this.client.UploadFiles(fileList);

            List<Property<FileRecord>> fileRecordList = new List<Property<FileRecord>>(fileWrapperMap.Count);

            for (int i = 0; i < mediaRecords.Length; i++)
            {
                MediaRecord mediaRecord = mediaRecords[i];
                //get the fileWrapper property by index of file in the list uploaded.
                Property<FileWrapper> fileWrapperProp = fileWrapperMap[fileList[i].GetId()];
                //build FileRecord property
                Property<FileRecord> fileRecordProp = fileWrapperProp.Transform((f) => new FileRecord(mediaRecord, f.Info()));
                fileRecordList.Add(fileRecordProp);
            }


            Helpers.AssignObjects(fileRecordList);
        }

        public async Task UploadFilesInObject(object data)
        {
            Dictionary<string, Property<FileWrapper>> fileWrapperMap = Helpers.ExtractFileWrappers(data);
            if (fileWrapperMap.Count == 0) return;


            List<FileWrapper> fileList = new List<FileWrapper>(fileWrapperMap.Count);

            foreach (Property<FileWrapper> fileProperty in fileWrapperMap.Values)
                fileList.Add(fileProperty.Value);

            MediaRecord[] mediaRecords = await this.client.UploadFiles(fileList);

            List<Property<FileRecord>> fileRecordList = new List<Property<FileRecord>>(fileWrapperMap.Count);

            for (int i = 0; i < mediaRecords.Length; i++)
            {
                MediaRecord mediaRecord = mediaRecords[i];
                //get the fileWrapper property by index of file in the list uploaded.
                Property<FileWrapper> fileWrapperProp = fileWrapperMap[fileList[i].GetId()];
                //build FileRecord property
                Property<FileRecord> fileRecordProp = fileWrapperProp.Transform((f) => new FileRecord(mediaRecord, f.Info()));
                fileRecordList.Add(fileRecordProp);
            }


            Helpers.AssignObjects(fileRecordList);
        }

        public async Task<TData> DownloadFilesInObject<TData>(TData data)
        {
            Dictionary<string, Property<FileRecord>> fileRecordMap = Helpers.ExtractFileRecords(data);
            List<Property<FileWrapper>> fileWrapperList = await this.DownloadFiles(fileRecordMap);
            Helpers.AssignObjects(fileWrapperList);
            return data;
        }

        private async Task<List<Property<FileWrapper>>> DownloadFiles(Dictionary<string, Property<FileRecord>> fileRecordMap)
        {
            List<Property<FileWrapper>> fileWrapperList = new List<Property<FileWrapper>>();
            if (fileRecordMap.Count == 0)
                return fileWrapperList;

            foreach (var fileRecordProp in fileRecordMap)
            {
                FileRecord fileRecord = fileRecordProp.Value.Value;
                MemoryStream file = await client.DownloadFile(fileRecord);
                FileInfo info = fileRecord.GetFileInfo();
                // When downloading a file, set disableEncrpytion to true if no key is passed so that the
                // FileWrapper doesn't generate a new key and try to decrypt the file data.
                FileWrapper fileWrapper = new FileBlobWrapper(file, info, info.Key == null || info.Key == "");
                fileWrapperList.Add(fileRecordProp.Value.Transform((T) => fileWrapper));
            }
            return fileWrapperList;
        }


        /// <summary>
        /// The MakeTraceState
        /// </summary>
        /// <typeparam name="TLinkData"></typeparam>
        /// <param name="trace">The trace<see cref="dynamic"/></param>
        /// <returns>The <see cref="TraceState{TState, TLinkData}"/></returns>
        private TraceState<TState, TLinkData> MakeTraceState<TLinkData>(dynamic trace)
        {
            JObject raw = trace.head.raw;

            var data = trace.head.data;

            string rawJson = raw.ToString(Newtonsoft.Json.Formatting.None);

            var linkObj = JsonHelper.FromJson<Link>(rawJson);

            TraceLink<TLinkData> headLink = new TraceLink<TLinkData>(new Stratumn.Chainscript.Link(linkObj), JsonHelper.ObjectToObject<TLinkData>(data));


            TraceState<TState, TLinkData> traceState = new TraceState<TState, TLinkData>(headLink.TraceId(), headLink, headLink.CreatedAt(),
              headLink.CreatedBy(), JsonHelper.ObjectToObject<TState>(trace.state.data), trace.tags ?? new string[0], headLink.Group()
           );

            return traceState;
        }

        /// <summary>
        /// The NewTrace
        /// </summary>
        /// <typeparam name="TLinkData"></typeparam>
        /// <param name="input">The input<see cref="NewTraceInput{TLinkData}"/></param>
        /// <returns>The <see cref="Task{TraceState{TState, TLinkData}}"/></returns>
        public async Task<TraceState<TState, TLinkData>> NewTraceAsync<TLinkData>(NewTraceInput<TLinkData> input)
        {
            //extract info from input
            string actionKey = input.ActionKey;
            TLinkData data = input.Data;
            string groupLabel = input.GroupLabel;

            SdkConfig sdkConfig = await this.GetConfigAsync();

            string workflowId = sdkConfig.WorkflowId;
            string configId = sdkConfig.ConfigId;
            string accountId = sdkConfig.AccountId;
            string groupId = sdkConfig.GetGroupId(groupLabel);

            // upload files and transform data
            await this.UploadFilesInLinkData(data);

            TraceLinkBuilderConfig<TLinkData> cfg = new TraceLinkBuilderConfig<TLinkData>()
            {
                WorkflowId = workflowId,
                // and workflow config id
                ConfigId = configId,
            };
            // use a TraceLinkBuilder to create the first link
            // only provide workflowId to initiate a new trace
            TraceLinkBuilder<TLinkData> linkBuilder = new TraceLinkBuilder<TLinkData>(cfg);

            // this is an attestation
            linkBuilder.ForAttestation(actionKey, data).WithGroup(groupId).WithCreatedBy(accountId);
            // call createLink helper
            return await this.CreateLinkAsync(linkBuilder);
        }

        /// <summary>
        /// The AppendLink
        /// </summary>
        /// <typeparam name="TLinkData"></typeparam>
        /// <param name="input">The input<see cref="AppendLinkInput{TLinkData}"/></param>
        /// <returns>The <see cref="Task{TraceState{TState, TLinkData}}"/></returns>
        public async Task<TraceState<TState, TLinkData>> AppendLinkAsync<TLinkData>(AppendLinkInput<TLinkData> input)
        {

            // retrieve parent link
            TransferResponseInput<TLinkData> headLinkInput = new TransferResponseInput<TLinkData>(input.TraceId, null);
            TraceLink<TLinkData> parentLink = await this.GetHeadLinkAsync<TLinkData>(headLinkInput);

            //extract info from input
            string actionKey = input.ActionKey;
            TLinkData data = input.Data;
            string groupLabel = input.GroupLabel;

            SdkConfig sdkConfig = await this.GetConfigAsync();

            string workflowId = sdkConfig.WorkflowId;
            string configId = sdkConfig.ConfigId;
            string accountId = sdkConfig.AccountId;
            string groupId = sdkConfig.GetGroupId(groupLabel);
            // upload files and transform data
            await this.UploadFilesInLinkData(data);

            TraceLinkBuilderConfig<TLinkData> cfg = new TraceLinkBuilderConfig<TLinkData>()
            {
                // provide workflow id
                WorkflowId = workflowId,
                // and workflow config id
                ConfigId = configId,
                // and parent link to append to the existing trace
                ParentLink = parentLink
            };
            // use a TraceLinkBuilder to create the first link
            // only provide workflowId to initiate a new trace
            TraceLinkBuilder<TLinkData> linkBuilder = new TraceLinkBuilder<TLinkData>(cfg);

            // this is an attestation
            linkBuilder.ForAttestation(actionKey, data)
                    .WithGroup(groupId)
                    .WithCreatedBy(accountId);
            // call createLink helper
            return await this.CreateLinkAsync(linkBuilder);
        }

        /// <summary>
        /// The PushTrace
        /// </summary>
        /// <typeparam name="TLinkData"></typeparam>
        /// <param name="input">The input<see cref="PushTransferInput{TLinkData}"/></param>
        /// <returns>The <see cref="Task{TraceState{TState, TLinkData}}"/></returns>
        public async Task<TraceState<TState, TLinkData>> PushTraceAsync<TLinkData>(PushTransferInput<TLinkData> input)
        {
            // retrieve parent link
            TransferResponseInput<TLinkData> headLinkInput = new TransferResponseInput<TLinkData>(input.TraceId, null);
            TraceLink<TLinkData> parentLink = await this.GetHeadLinkAsync<TLinkData>(headLinkInput);

            //extract info from input
            string recipient = input.Recipient;
            TLinkData data = input.Data;

            SdkConfig sdkConfig = await this.GetConfigAsync();

            string workflowId = sdkConfig.WorkflowId;
            string configId = sdkConfig.ConfigId;
            string accountId = sdkConfig.AccountId;

            TraceLinkBuilderConfig<TLinkData> cfg = new TraceLinkBuilderConfig<TLinkData>()
            {
                // provide workflow id
                WorkflowId = workflowId,
                // and workflow config id
                ConfigId = configId,
                // and parent link to append to the existing trace
                ParentLink = parentLink
            };
            // use a TraceLinkBuilder to create the first link
            // only provide workflowId to initiate a new trace
            TraceLinkBuilder<TLinkData> linkBuilder = new TraceLinkBuilder<TLinkData>(cfg);

            // this is a push transfer
            linkBuilder.ForPushTransfer(recipient, data).WithCreatedBy(accountId);
            // call createLink helper
            return await this.CreateLinkAsync(linkBuilder);
        }

        /// <summary>
        /// The CancelTransfer
        /// </summary>
        /// <typeparam name="TLinkData"></typeparam>
        /// <param name="input">The input<see cref="TransferResponseInput{TLinkData}"/></param>
        /// <returns>The <see cref="Task{TraceState{TState, TLinkData}}"/></returns>
        [Obsolete("CancelTransferAsync is deprecated")]
        public async Task<TraceState<TState, TLinkData>> CancelTransferAsync<TLinkData>(TransferResponseInput<TLinkData> input)
        {
            // retrieve parent link
            TransferResponseInput<TLinkData> headLinkInput = new TransferResponseInput<TLinkData>(input.TraceId, null);
            TraceLink<TLinkData> parentLink = await this.GetHeadLinkAsync<TLinkData>(headLinkInput);

            TLinkData data = input.Data;

            SdkConfig sdkConfig = await this.GetConfigAsync();

            String workflowId = sdkConfig.WorkflowId;
            String configId = sdkConfig.ConfigId;
            String accountId = sdkConfig.AccountId;

            TraceLinkBuilderConfig<TLinkData> cfg = new TraceLinkBuilderConfig<TLinkData>()
            {
                // provide workflow id
                WorkflowId = workflowId,
                // and workflow config id
                ConfigId = configId,
                // and parent link to append to the existing trace
                ParentLink = parentLink
            };
            // use a TraceLinkBuilder to create the first link
            // only provide workflowId to initiate a new trace
            TraceLinkBuilder<TLinkData> linkBuilder = new TraceLinkBuilder<TLinkData>(cfg);

            linkBuilder // this is to cancel the transfer
               .ForCancelTransfer(data)
               // add creator info
               .WithCreatedBy(accountId);
            // call createLink helper
            return await this.CreateLinkAsync(linkBuilder);
        }

        /// <summary>
        /// The GetTraceState
        /// </summary>
        /// <typeparam name="TLinkData"></typeparam>
        /// <param name="input">The input<see cref="GetTraceStateInput"/></param>
        /// <returns>The <see cref="Task{TraceState{TState, TLinkData}}"/></returns>
        public async Task<TraceState<TState, TLinkData>> GetTraceStateAsync<TLinkData>(GetTraceStateInput input)
        {
            Dictionary<string, object> var = new Dictionary<string, object>
            {
                { "traceId", input.TraceId }
            };
            // create variables
            GraphQLResponse<dynamic> jsonResponse = await this.client.GraphqlAsync(GraphQL.QUERY_GETTRACESTATE, var, null, null);

            var trace = jsonResponse.Data.trace;

            return this.MakeTraceState<TLinkData>(trace);
        }

        /// <summary>
        /// The GetTraceDetails
        /// </summary>
        /// <typeparam name="TLinkData"></typeparam>
        /// <param name="input">The input<see cref="GetTraceDetailsInput"/></param>
        /// <returns>The <see cref="Task{TraceDetails{TLinkData}}"/></returns>
        public async Task<TraceDetails<TLinkData>> GetTraceDetailsAsync<TLinkData>(GetTraceDetailsInput input)
        {
            Dictionary<string, object> getTraceDetailsInput = JsonHelper.ObjectToMap(input);


            // delegate the graphql request execution 

            // execute graphql query
            GraphQLResponse<dynamic> jsonResponse = await this.client.GraphqlAsync(GraphQL.QUERY_GETTRACEDETAILS, getTraceDetailsInput, null, null);


            var trace = jsonResponse.Data.trace;


            var info = trace.links.info;
            int totalCount = (int)trace.links.totalCount;

            IList<TraceLink<TLinkData>> links = new List<TraceLink<TLinkData>>();

            // get all the groups that are owned by one of my accounts
            foreach (var lNode in trace.links.nodes)
            {

                JObject nodeRaw = lNode.raw;
                TLinkData nodeData = JsonHelper.ObjectToObject<TLinkData>(lNode.data);
                links.Add((TraceLink<TLinkData>)TraceLink<TLinkData>.FromObject<TLinkData>(nodeRaw.ToString(Newtonsoft.Json.Formatting.None),
                                                                                                                         nodeData));

            }

            // the details response object
            return new TraceDetails<TLinkData>(links, totalCount, info.ToObject<Info>());
        }

        /// <summary>
        /// The GetIncomingTracesAsync
        /// </summary>
        /// <typeparam name="TLinkData"></typeparam>
        /// <param name="paginationInfo">The paginationInfo<see cref="PaginationInfo"/></param>
        /// <returns>The <see cref="Task{TracesState{TState, TLinkData}}"/></returns>
        public async Task<TracesState<TState, TLinkData>> GetIncomingTracesAsync<TLinkData>(PaginationInfo paginationInfo)
        {
            return await this.GetTracesInStageAsync<TLinkData>(TraceStageType.INCOMING, paginationInfo, null);
        }

        /// <summary>
        /// The GetOutgoingTracesAsync
        /// </summary>
        /// <typeparam name="TLinkData"></typeparam>
        /// <param name="paginationInfo">The paginationInfo<see cref="PaginationInfo"/></param>
        /// <returns>The <see cref="Task{TracesState{TState, TLinkData}}"/></returns>
        public async Task<TracesState<TState, TLinkData>> GetOutgoingTracesAsync<TLinkData>(PaginationInfo paginationInfo)
        {
            return await this.GetTracesInStageAsync<TLinkData>(TraceStageType.OUTGOING, paginationInfo, null);
        }

        /// <summary>
        /// The GetBacklogTracesAsync
        /// </summary>
        /// <typeparam name="TLinkData"></typeparam>
        /// <param name="paginationInfo">The paginationInfo<see cref="PaginationInfo"/></param>
        /// <returns>The <see cref="Task{TracesState{TState, TLinkData}}"/></returns>
        public async Task<TracesState<TState, TLinkData>> GetBacklogTracesAsync<TLinkData>(PaginationInfo paginationInfo)
        {
            return await this.GetTracesInStageAsync<TLinkData>(TraceStageType.BACKLOG, paginationInfo, null);
        }

        /// <summary>
        /// The GetAttestationTracesAsync
        /// </summary>
        /// <typeparam name="TLinkData"></typeparam>
        /// <param name="actionKey">The action key<see cref="string"/></param>
        /// <param name="paginationInfo">The paginationInfo<see cref="PaginationInfo"/></param>
        /// <returns>The <see cref="Task{TracesState{TState, TLinkData}}"/></returns>
        public async Task<TracesState<TState, TLinkData>> GetAttestationTracesAsync<TLinkData>(string actionKey, PaginationInfo paginationInfo)
        {
            return await this.GetTracesInStageAsync<TLinkData>(TraceStageType.ATTESTATION, paginationInfo, actionKey);
        }

        /// <summary>
        /// The AcceptTransferAsync
        /// </summary>
        /// <typeparam name="TLinkData"></typeparam>
        /// <param name="input">The input<see cref="TransferResponseInput{TLinkData}"/></param>
        /// <returns>The <see cref="Task{TraceState{TState, TLinkData}}"/></returns>
        [Obsolete("AcceptTransferAsync is deprecated")]
        public async Task<TraceState<TState, TLinkData>> AcceptTransferAsync<TLinkData>(TransferResponseInput<TLinkData> input)
        {
            // retrieve parent link 
            TraceLink<TLinkData> parentLink = await this.GetHeadLinkAsync(input);

            //extract info from input
            TLinkData data = input.Data;
            string groupLabel = input.GroupLabel;

            SdkConfig sdkConfig = await this.GetConfigAsync();

            String workflowId = sdkConfig.WorkflowId;
            string configId = sdkConfig.ConfigId;
            String accountId = sdkConfig.AccountId;
            String groupId = sdkConfig.GetGroupId(groupLabel);

            TraceLinkBuilderConfig<TLinkData> cfg = new TraceLinkBuilderConfig<TLinkData>()
            {
                // provide workflow id
                WorkflowId = workflowId,
                // and workflow config id
                ConfigId = configId,
                // and parent link to append to the existing trace
                ParentLink = parentLink
            };
            // use a TraceLinkBuilder to create the first link
            // only provide workflowId to initiate a new trace
            TraceLinkBuilder<TLinkData> linkBuilder = new TraceLinkBuilder<TLinkData>(cfg);

            // this is an attestation
            linkBuilder.ForAcceptTransfer(data)
               // add group info
               .WithGroup(groupId)
               // add creator info
               .WithCreatedBy(accountId);
            // call createLink helper
            return await CreateLinkAsync(linkBuilder);
        }

        /// <summary>
        /// The RejectTransferAsync
        /// </summary>
        /// <typeparam name="TLinkData"></typeparam>
        /// <param name="input">The input<see cref="TransferResponseInput{TLinkData}"/></param>
        /// <returns>The <see cref="Task{TraceState{TState, TLinkData}}"/></returns>
        [Obsolete("RejectTransferAsync is deprecated")]
        public async Task<TraceState<TState, TLinkData>> RejectTransferAsync<TLinkData>(TransferResponseInput<TLinkData> input)
        {

            // retrieve parent link
            TransferResponseInput<TLinkData> headLinkInput = new TransferResponseInput<TLinkData>(input.TraceId, null);
            TraceLink<TLinkData> parentLink = await this.GetHeadLinkAsync<TLinkData>(headLinkInput);

            TLinkData data = input.Data;
            string groupLabel = input.GroupLabel;

            SdkConfig sdkConfig = await this.GetConfigAsync();

            String workflowId = sdkConfig.WorkflowId;
            string configId = sdkConfig.ConfigId;
            String accountId = sdkConfig.AccountId;
            String groupId = sdkConfig.GetGroupId(groupLabel);

            TraceLinkBuilderConfig<TLinkData> cfg = new TraceLinkBuilderConfig<TLinkData>()
            {
                // provide workflow id
                WorkflowId = workflowId,
                // and workflow config id
                ConfigId = configId,
                // and parent link to append to the existing trace
                ParentLink = parentLink
            };
            // use a TraceLinkBuilder to create the first link
            // only provide workflowId to initiate a new trace
            TraceLinkBuilder<TLinkData> linkBuilder = new TraceLinkBuilder<TLinkData>(cfg);

            // this is a push transfer
            linkBuilder.ForRejectTransfer(data)
               // add group info
               .WithGroup(groupId)
               // add creator info
               .WithCreatedBy(accountId);
            // call createLink helper
            return await this.CreateLinkAsync(linkBuilder);
        }


        public async Task<TraceState<TState, TLinkData>> AddTagsToTraceAsync<TLinkData>(AddTagsToTraceInput input)
        {
            // build variables
            Dictionary<String, object> variables = new Dictionary<String, object>
            {
                { "traceId", input.TraceId },
                { "tags", input.Tags }
            };

            // execute graphql query
            string query = GraphQL.MUTATION_ADDTAGSTOTRACE;
            GraphQLResponse<dynamic> jsonResponse = await this.client.GraphqlAsync(query, variables, null, null);

            return this.MakeTraceState<TLinkData>((JObject)jsonResponse.Data.addTagsToTrace.trace);
        }

        /// <summary>
        /// Search all the traces of the workflow
        /// </summary>
        /// <typeparam name="SearchTracesFilter"></typeparam>
        /// <param name="filter">the filter to use in the search</param>
        /// <typeparam name="PaginationInfo"></typeparam>
        /// <param name="paginationInfo">the wanted pagination</param>
        /// <returns>The <see cref="Task{TraceState{TState, TLinkData}}"/></returns>
        public async Task<TracesState<TState, TLinkData>> SearchTracesAsync<TLinkData>(SearchTracesFilter filter,
                 PaginationInfo paginationInfo)
        {

            // create variables
            Dictionary<String, object> variables = new Dictionary<String, object>{
                { "filter", filter.GetFilters() },
                { "workflowId", this.opts.WorkflowId }
            };
            Dictionary<String, object> variablesPaginationInfo = JsonHelper.ObjectToMap(paginationInfo);
            variablesPaginationInfo.ToList().ForEach(x => variables.Add(x.Key, x.Value));

            // execute graphql query
            string query = GraphQL.QUERY_SEARCHTRACES;
            GraphQLResponse<dynamic> jsonResponse = await this.client.GraphqlAsync(query, variables, null, null);

            // get all the traces
            var traceResponse = jsonResponse.Data.workflow.traces;

            List<TraceState<TState, TLinkData>> traces = new List<TraceState<TState, TLinkData>>();
            var nodes = traceResponse.nodes;

            foreach (var node in nodes)
            {
                traces.Add(this.MakeTraceState<TLinkData>((JObject)node));
            }

            var info = traceResponse.info;
            int totalCount = (int)traceResponse.totalCount;

            // construct the traces list object
            TracesState<TState, TLinkData> tracesList = new TracesState<TState, TLinkData>()
            {
                Traces = traces,
                TotalCount = totalCount,
                Info = info.ToObject<Info>()
            };

            return tracesList;
        }

    }
}
