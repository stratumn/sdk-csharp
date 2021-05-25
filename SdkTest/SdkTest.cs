using Xunit;
using System;
using System.Text;
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
    public class SdkTest
    {

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

        public Sdk<object> GetSdk()
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
            Sdk<object> sdk = new Sdk<object>(opts);

            return sdk;
        }

        public Sdk<object> GetOtherGroupSdk()
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
            opts.GroupLabel = ConfigTest.MY_GROUP_LABEL;
            Sdk<object> sdk = new Sdk<object>(opts);

            return sdk;
        }

        [Fact]
        public async Task LoginWithPrivateKeyDemo()
        {
            var sdk = GetSdk();
            string token = await sdk.LoginAsync();
            Debug.WriteLine(token);
            Assert.NotNull(token);
        }

        [Fact]
        public async Task LoginWithCredentialsDemo()
        {
            var sdk = GetSdk();
            string token = await sdk.LoginAsync();
            Debug.WriteLine(token);
            Assert.NotNull(token);
        }

        [Fact]
        public async Task TestGetConfigAsync()
        {
            var sdk = GetSdk();
            SdkConfig config = await sdk.GetConfigAsync();
            Assert.NotNull(config);
        }

        [Fact]
        public async Task GetTraceStateTest()
        {
            Sdk<object> sdk = GetSdk();
            string traceId = ConfigTest.TRACE_ID;
            GetTraceStateInput input = new GetTraceStateInput(traceId);
            TraceState<object, object> state = await sdk.GetTraceStateAsync<object>(input);
            Assert.Equal(state.TraceId, traceId);
        }

        [Fact]
        public async Task GetTraceDetails()
        {
            var sdk = GetSdk();
            string traceId = ConfigTest.TRACE_ID;

            GetTraceDetailsInput input = new GetTraceDetailsInput(traceId, 5, null, null, null);

            TraceDetails<object> details = await sdk.GetTraceDetailsAsync<object>(input);
            Debug.WriteLine(JsonHelper.ToJson(details));
            Assert.NotNull(details);
        }

        //used to pass the trace from one test method to another
        private TraceState<object, object> someTraceState;
        private TraceState<object, object> traceStateWithFile;

        public async Task NewTraceTest()
        {
            var sdk = GetSdk();
            IDictionary<string, object> data = new Dictionary<string, object>
            {
                ["entity"] = ConfigTest.OTHER_GROUP_NAME,
                ["submissionPeriod"] = "2021.Q4",
                ["startDate"] = "2021-01-30",
                ["deadline"] = "2021-06-30",
                ["comment"] = "init comment"
            };
            NewTraceInput<object> input = new NewTraceInput<object>(ConfigTest.INIT_ACTION_KEY, data);

            TraceState<object, object> state = await sdk.NewTraceAsync<object>(input);
            someTraceState = state;
        }

        [Fact]
        public async Task AppendLinkTest()
        {
            await NewTraceTest();
            Assert.NotNull(someTraceState);
            Dictionary<string, object> data = new Dictionary<string, object>
            {
                ["comment"] = "comment"
            };
            AppendLinkInput<object> appLinkInput = new AppendLinkInput<object>(ConfigTest.COMMENT_ACTION_KEY, data, someTraceState.TraceId);
            TraceState<object, object> state = await GetSdk().AppendLinkAsync(appLinkInput);
            Assert.NotNull(state.TraceId);
        }

        [Fact]
        public async Task NewTraceUploadTest()
        {
            await NewTraceTest();

            IDictionary<string, object> data = new Dictionary<string, object>();
            data.Add("documents", new Identifiable[] { FileWrapper.FromFilePath(GetTestFilePath()) });

            AppendLinkInput<object> appLinkInput = new AppendLinkInput<object>(ConfigTest.UPLOAD_DOCUMENTS_ACTION_KEY, data, someTraceState.TraceId);
            TraceState<object, object> state = await GetOtherGroupSdk().AppendLinkAsync<object>(appLinkInput);
            Assert.NotNull(state.TraceId);
            traceStateWithFile = state;
        }

        [Fact]
        public async Task UploadFilesInLinkDataTest()
        {
            await NewTraceTest();
            IDictionary<string, object> data = new Dictionary<string, object>
            {
                ["comment"] = "upload file comment"
            };
            data.Add("documents", new Identifiable[] { FileWrapper.FromFilePath(GetTestFilePath()) });

            await GetOtherGroupSdk().UploadFilesInLinkData(data);

            Assert.True(FileRecord.IsFileRecord(((object[])data["documents"])[0]));

            Debug.WriteLine(JsonHelper.ToJson(data));
        }

        [Fact]
        public async Task downloadFilesInObjectTest()
        {
            TraceState<Object, Object> state;
            if (traceStateWithFile == null)
            {
                await NewTraceUploadTest();
            }
            state = traceStateWithFile;

            Object dataWithRecords = state.HeadLink.FormData();

            object dataWithFiles = await GetSdk().DownloadFilesInObject(dataWithRecords);
            IDictionary<String, Property<FileWrapper>> fileWrappers = Helpers.ExtractFileWrappers(dataWithFiles);

            foreach (Property<FileWrapper> fileWrapperProp in fileWrappers.Values)
            {
                // Verify the contents of the file
                byte[] decrypted = fileWrapperProp.Value.DecryptedData().ToArray();
                byte[] expected = Encoding.UTF8.GetBytes("\nThis is a test file");
                Assert.Equal(decrypted, expected);
            }
        }

        [Fact]
        public async Task ImportDataCsvTest()
        {
            await NewTraceTest();

            IDictionary<string, object> data = new Dictionary<string, object>();

            // dirty simulate loading from csv
            IDictionary<string, object> taSummary = new Dictionary<string, object>();
            string json = "[{ reference: \"reference\", entityName: \"entity\", currency: \"EUR\", amount: 500, endDate: \"2020-06-25\"},"
            + "{reference: \"reference 2\", entityName: \"entity 2\", currency: \"EUR\", amount: 1300, endDate: \"2020-06-28\""
          + "}]";

            data.Add("taSummary", JsonHelper.FromJson<Object>(json));
            data.Add("file", FileWrapper.FromFilePath(GetTaFilePath()));

            AppendLinkInput<object> appLinkInput = new AppendLinkInput<object>(ConfigTest.IMPORT_TA_ACTION_KEY, data, someTraceState.TraceId);
            TraceState<object, object> state = await GetSdk().AppendLinkAsync<object>(appLinkInput);
            Assert.NotNull(state.TraceId);
        }

        [Fact]
        public async Task traceTagsRWTest()
        {
            Sdk<object> sdk = GetSdk();

            // Add a tag to a trace
            string traceId = ConfigTest.TRACE_ID;
            Guid uuid = System.Guid.NewGuid();
            string randomUUIDString = uuid.ToString();
            AddTagsToTraceInput input = new AddTagsToTraceInput(traceId, new string[] { randomUUIDString, "tag1", "tag2" });

            TraceState<Object, Object> state = await sdk.AddTagsToTraceAsync<Object>(input);

            Assert.Equal(traceId, state.TraceId);

            // search the trace by tags
            List<String> tags = new List<string>();
            tags.Add(randomUUIDString);
            SearchTracesFilter f = new SearchTracesFilter(tags);
            TracesState<Object, Object> res = await sdk.SearchTracesAsync<Object>(f, new PaginationInfo());

            Assert.Equal(1, res.TotalCount);
            Assert.Equal(traceId, res.Traces[0].TraceId);
        }

        [Fact]
        public async Task searchByMultipletagsTest()
        {
            Sdk<Object> sdk = GetSdk();

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
            await NewTraceTest();
            Assert.NotNull(someTraceState);
            Assert.Equal(someTraceState.UpdatedByGroupId, ConfigTest.MY_GROUP);
            Sdk<Object> sdk = GetSdk();
            // Appendlink
            Dictionary<string, object> dataMap = new Dictionary<string, object>
            {
                ["comment"] = "commment"
            };

            AppendLinkInput<Dictionary<string, object>> appLinkInput = new AppendLinkInput<Dictionary<string, object>>(
                  ConfigTest.COMMENT_ACTION_KEY, dataMap, someTraceState.TraceId);
            // change group for action
            appLinkInput.GroupLabel = ConfigTest.OTHER_GROUP_LABEL;
            TraceState<Object, Dictionary<string, object>> state = await sdk.AppendLinkAsync(appLinkInput);
            // should equal group2 id
            Assert.Equal(state.UpdatedByGroupId, ConfigTest.OTHER_GROUP);
            AppendLinkInput<Dictionary<string, object>> appLinkInputWithGroupLabel = new AppendLinkInput<Dictionary<string, object>>(
                  ConfigTest.COMMENT_ACTION_KEY, dataMap, someTraceState.TraceId);
            appLinkInputWithGroupLabel.GroupLabel = ConfigTest.MY_GROUP_LABEL;
            state = await sdk.AppendLinkAsync(appLinkInputWithGroupLabel);
            // should equal group2 id
            Assert.Equal(state.UpdatedByGroupId, ConfigTest.MY_GROUP);
        }
    }
}
