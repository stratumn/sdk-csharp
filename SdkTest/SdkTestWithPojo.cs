using Xunit;
using System;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.IO;

using Stratumn.Sdk;
using Stratumn.Sdk.Model.Client;
using Stratumn.Sdk.Model.Sdk;
using Stratumn.Sdk.Model.Trace;
using Stratumn.Chainscript.utils;
using Stratumn.Sdk.Model.Misc;

namespace SdkTest
{
    public class SdkTestPojoTest
    {
        // used to pass the trace from one test method to another
        private TraceState<StateExample, InitDataClass> initTraceState;

        private string GetTestFilePath(
            // This is a weird hack to get the location of this source file
            // https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.callerfilepathattribute?view=netstandard-2.0
            [CallerFilePath] string callerFilePath = ""
        )
        {
            return Path.Combine(
                Directory.GetParent(callerFilePath).FullName,
                "Resources",
                "TestFile1.txt"
            );
        }

        private string GetTaFilePath(
           // This is a weird hack to get the location of this source file
           // https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.callerfilepathattribute?view=netstandard-2.0
           [CallerFilePath] string callerFilePath = ""
       )
        {
            return Path.Combine(
                Directory.GetParent(callerFilePath).FullName,
                "Resources",
                "TA.csv"
            );
        }

        public Sdk<T> GetSdk<T>()
        {
            Secret s = Secret.NewPrivateKeySecret(ConfigTest.PEM_PRIVATEKEY);

            SdkOptions opts = new SdkOptions(ConfigTest.WORKFLOW_ID, s);
            opts.Endpoints = new Endpoints
            {
                Trace = ConfigTest.TRACE_URL,
                Account = ConfigTest.ACCOUNT_URL,
                Media = ConfigTest.MEDIA_URL,
            };
            opts.EnableDebuging = true;
            opts.GroupLabel = ConfigTest.MY_GROUP_LABEL;
            Sdk<T> sdk = new Sdk<T>(opts);

            return sdk;
        }

        public Sdk<T> GetOtherGroupSdk<T>()
        {
            Secret s = Secret.NewPrivateKeySecret(ConfigTest.PEM_PRIVATEKEY_2);

            SdkOptions opts = new SdkOptions(ConfigTest.WORKFLOW_ID, s);
            opts.Endpoints = new Endpoints
            {
                Trace = ConfigTest.TRACE_URL,
                Account = ConfigTest.ACCOUNT_URL,
                Media = ConfigTest.MEDIA_URL,
            };
            opts.EnableDebuging = true;
            opts.GroupLabel = ConfigTest.OTHER_GROUP_LABEL;
            Sdk<T> sdk = new Sdk<T>(opts);

            return sdk;
        }

        public class InitDataClass
        {
            public string entity;
            public string submissionPeriod;
            public string startDate;
            public string deadline;
            public string comment;

        }

        public class CommentClass
        {
            public string comment;
        }

        public class UploadDocumentsClass
        {
            public Identifiable[] documents;
            public string comment;
        }

        public class StatusClass
        {
            public string value;
            public double progress;
        }

        public class ImportTaClass
        {
            public Object taSummary;
            public Object file;
        }

        public class StateExample
        {
            public string entity;
            public string submissionPeriod;
            public string startDate;
            public string deadline;
            public Object taSummary;
            public StatusClass status;
            public Object[] comments;

        }
        public class HeadLinkData
        {
            public bool Valid { get; set; }
            public string Weight { get; set; }
            public string Operation { get; set; }
            public String[] Operators;
        }

        [Fact]
        public async Task GetTraceStateTestWithPojo()
        {
            Sdk<StateExample> sdk = GetSdk<StateExample>();
            GetTraceStateInput input = new GetTraceStateInput(ConfigTest.TRACE_ID);
            TraceState<StateExample, HeadLinkData> state = await sdk.GetTraceStateAsync<HeadLinkData>(input);
            Assert.Equal(state.TraceId, ConfigTest.TRACE_ID);
        }

        [Fact]
        public async Task GetTraceDetailsWithPojo()
        {
            Sdk<StateExample> sdk = GetSdk<StateExample>();

            GetTraceDetailsInput input = new GetTraceDetailsInput(ConfigTest.TRACE_ID, 5, null, null, null);

            TraceDetails<HeadLinkData> details = await sdk.GetTraceDetailsAsync<HeadLinkData>(input);
            Debug.WriteLine(JsonHelper.ToJson(details));
            Assert.NotNull(details);
        }

        [Fact]
        public async Task NewTraceTestWithPojo()
        {
            var sdk = GetSdk<StateExample>();

            InitDataClass data = new InitDataClass();
            data.entity = ConfigTest.OTHER_GROUP_NAME;
            data.submissionPeriod = "2021.Q4";
            data.startDate = "2021-01-30";
            data.deadline = "2021-06-30";
            data.comment = "init comment";

            NewTraceInput<InitDataClass> input = new NewTraceInput<InitDataClass>(ConfigTest.INIT_ACTION_KEY, data);

            TraceState<StateExample, InitDataClass> state = await sdk.NewTraceAsync<InitDataClass>(input);
            initTraceState = state;
            Debug.WriteLine(JsonHelper.ToJson(state));
            Assert.NotNull(state.TraceId);
        }

        [Fact]
        public async Task AppendLinkTestWithPojo()
        {
            var sdk = GetSdk<StateExample>();
            await NewTraceTestWithPojo();
            Assert.NotNull(initTraceState);
            CommentClass data = new CommentClass();
            data.comment = "comment";

            AppendLinkInput<CommentClass> appLinkInput = new AppendLinkInput<CommentClass>(ConfigTest.COMMENT_ACTION_KEY, data, initTraceState.TraceId);
            TraceState<StateExample, CommentClass> state = await sdk.AppendLinkAsync<CommentClass>(appLinkInput);
            Assert.NotNull(state.TraceId);
        }

        [Fact]
        public async Task NewTraceUploadInClassTest()
        {
            await NewTraceTestWithPojo();
            UploadDocumentsClass data = new UploadDocumentsClass();
            data.documents = new Identifiable[] { FileWrapper.FromFilePath(GetTestFilePath()) };
            data.comment = "upload comment";

            AppendLinkInput<UploadDocumentsClass> appLinkInput = new AppendLinkInput<UploadDocumentsClass>(ConfigTest.UPLOAD_DOCUMENTS_ACTION_KEY, data, initTraceState.TraceId);
            TraceState<StateExample, UploadDocumentsClass> state = await GetOtherGroupSdk<StateExample>().AppendLinkAsync<UploadDocumentsClass>(appLinkInput);
            Assert.NotNull(state.TraceId);
        }

        [Fact]
        public async Task ImportDataCsvTest()
        {
            await NewTraceTestWithPojo();

            ImportTaClass data = new ImportTaClass();
            // dirty simulate loading from csv
            IDictionary<string, object> taSummary = new Dictionary<string, object>();
            string json = "[{ reference: \"reference\", entityName: \"entity\", currency: \"EUR\", amount: 500, endDate: \"2020-06-25\"},"
            + "{reference: \"reference 2\", entityName: \"entity 2\", currency: \"EUR\", amount: 1300, endDate: \"2020-06-28\""
          + "}]";

            data.taSummary = JsonHelper.FromJson<Object>(json);
            data.file = FileWrapper.FromFilePath(GetTaFilePath());

            AppendLinkInput<ImportTaClass> appLinkInput = new AppendLinkInput<ImportTaClass>(ConfigTest.IMPORT_TA_ACTION_KEY, data, initTraceState.TraceId);
            TraceState<StateExample, ImportTaClass> state = await GetSdk<StateExample>().AppendLinkAsync<ImportTaClass>(appLinkInput);
            Assert.NotNull(state.TraceId);
        }

        [Fact]
        public async Task traceTagsRWTest()
        {
            Sdk<StateExample> sdk = GetSdk<StateExample>();

            // Add a tag to a trace
            string traceId = ConfigTest.TRACE_ID;
            Guid uuid = System.Guid.NewGuid();
            string randomUUIDString = uuid.ToString();
            AddTagsToTraceInput input = new AddTagsToTraceInput(traceId, new string[] { randomUUIDString, "tag1", "tag2" });

            TraceState<StateExample, Object> state = await sdk.AddTagsToTraceAsync<Object>(input);

            Assert.Equal(traceId, state.TraceId);

            // search the trace by tags
            List<String> tags = new List<string>();
            tags.Add(randomUUIDString);
            SearchTracesFilter f = new SearchTracesFilter(tags);
            TracesState<StateExample, Object> res = await sdk.SearchTracesAsync<Object>(f, new PaginationInfo());

            Assert.Equal(1, res.TotalCount);
            Assert.Equal(traceId, res.Traces[0].TraceId);
        }

        [Fact]
        public async Task searchByMultipletagsTest()
        {
            Sdk<Object> sdk = GetSdk<Object>();

            // search the trace by tags
            List<String> tags = new List<string>();
            tags.Add("tag1");
            tags.Add("tag2");
            SearchTracesFilter f = new SearchTracesFilter();
            f.Tags = tags;
            f.SearchType = SearchTracesFilter.SEARCH_TYPE.TAGS_CONTAINS;
            TracesState<Object, Object> res = await sdk.SearchTracesAsync<Object>(f, new PaginationInfo());

            Assert.Equal(1, res.TotalCount);
            Assert.Equal(ConfigTest.TRACE_ID, res.Traces[0].TraceId);
        }

        [Fact]
        public async Task changeGroupTest()
        {
            await NewTraceTestWithPojo();
            Assert.NotNull(initTraceState);
            Assert.Equal(initTraceState.UpdatedByGroupId, ConfigTest.MY_GROUP);
            Sdk<StateExample> sdk = GetSdk<StateExample>();
            // Appendlink
            CommentClass dataMap = new CommentClass()
            {
                comment = "commment"
            };

            AppendLinkInput<CommentClass> appLinkInput = new AppendLinkInput<CommentClass>(
                  ConfigTest.COMMENT_ACTION_KEY, dataMap, initTraceState.TraceId);
            // change group for action
            appLinkInput.GroupLabel = ConfigTest.OTHER_GROUP_LABEL;
            TraceState<StateExample, CommentClass> state = await sdk.AppendLinkAsync(appLinkInput);
            // should equal group2 id
            Assert.Equal(state.UpdatedByGroupId, ConfigTest.OTHER_GROUP);
            AppendLinkInput<CommentClass> appLinkInputWithGroupLabel = new AppendLinkInput<CommentClass>(
                  ConfigTest.COMMENT_ACTION_KEY, dataMap, initTraceState.TraceId);
            appLinkInputWithGroupLabel.GroupLabel = ConfigTest.MY_GROUP_LABEL;
            state = await sdk.AppendLinkAsync(appLinkInputWithGroupLabel);
            // should equal group2 id
            Assert.Equal(state.UpdatedByGroupId, ConfigTest.MY_GROUP);
        }
    }
}
