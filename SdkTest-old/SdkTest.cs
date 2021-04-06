using Xunit;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.IO;

using Stratumn.Chainscript.utils;
using Stratumn.Sdk;
using Stratumn.Sdk.Model.Client;
using Stratumn.Sdk.Model.Sdk;
using Stratumn.Sdk.Model.Trace;
using Stratumn.Sdk.Model.Misc;

namespace SDKTest
{
    public class SdkTest
    {
        private const string WORKFLOW_ID = "591";
        private const string ACTION_KEY = "action1";

        private const string TRACE_ID = "36adf228-c44c-429c-850f-db1910770d3e";

        private const string MY_GROUP = "1744";
        private const string PEM_PRIVATEKEY = "-----BEGIN ED25519 PRIVATE KEY-----\nMFACAQAwBwYDK2VwBQAEQgRACaNT4cup/ZQAq4IULZCrlPB7eR1QTCN9V3Qzct8S\nYp57BqN4FipIrGpyclvbT1FKQfYLJpeBXeCi2OrrQMTgiw==\n-----END ED25519 PRIVATE KEY-----\n";

        private static string PEM_PRIVATEKEY_2 = "-----BEGIN ED25519 PRIVATE KEY-----\nMFACAQAwBwYDK2VwBQAEQgRAWotrb1jJokHr7AVQTS6f6W7dFYnKpVy+DV++sG6x\nlExB4rtrKpCAEPt5q7oT6/lcF4brFSNiCxLPnHqiSjcyVw==\n-----END ED25519 PRIVATE KEY-----\n";
        private static string OTHER_GROUP = "1785";

        private const string TRACE_URL = "https://trace-api.staging.stratumn.com";
        private const string ACCOUNT_URL = "https://account-api.staging.stratumn.com";
        private const string MEDIA_URL = "https://media-api.staging.stratumn.com";

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

        public Sdk<T> GetSdk<T>()
        {
            Secret s = Secret.NewPrivateKeySecret(PEM_PRIVATEKEY);

            SdkOptions opts = new SdkOptions(WORKFLOW_ID, s);
            opts.Endpoints = new Endpoints
            {
                Trace = TRACE_URL,
                Account = ACCOUNT_URL,
                Media = MEDIA_URL,
            };
            opts.EnableDebuging = true;
            Sdk<T> sdk = new Sdk<T>(opts);

            return sdk;
        }

        public Sdk<object> GetSdk()
        {
            Secret s = Secret.NewPrivateKeySecret(PEM_PRIVATEKEY);
            SdkOptions opts = new SdkOptions(WORKFLOW_ID, s);
            opts.Endpoints = new Endpoints
            {
                Trace = TRACE_URL,
                Account = ACCOUNT_URL,
                Media = MEDIA_URL,
            };
            opts.EnableDebuging = true;
            Sdk<object> sdk = new Sdk<object>(opts);

            return sdk;
        }

        public Sdk<T> GetOtherGroupSdk<T>()
        {
            Secret s = Secret.NewPrivateKeySecret(PEM_PRIVATEKEY_2);

            SdkOptions opts = new SdkOptions(WORKFLOW_ID, s);
            opts.Endpoints = new Endpoints
            {
                Trace = TRACE_URL,
                Account = ACCOUNT_URL,
                Media = MEDIA_URL,
            };
            opts.EnableDebuging = true;
            Sdk<T> sdk = new Sdk<T>(opts);

            return sdk;
        }

        public Sdk<object> GetOtherGroupSdk()
        {
            Secret s = Secret.NewPrivateKeySecret(PEM_PRIVATEKEY_2);
            SdkOptions opts = new SdkOptions(WORKFLOW_ID, s);
            opts.Endpoints = new Endpoints
            {
                Trace = TRACE_URL,
                Account = ACCOUNT_URL,
                Media = MEDIA_URL,
            };
            opts.EnableDebuging = true;
            Sdk<object> sdk = new Sdk<object>(opts);

            return sdk;
        }

        [Fact]
        public async Task LoginWtihPrivateKeyDemo()
        {
            var sdk = GetSdk();
            string token = await sdk.LoginAsync();
            Debug.WriteLine(token);
            Assert.NotNull(token);
        }

        // used to pass the trace from one test method to another
        private TraceState<StateExample, SomeClass> someTraceState2;

        [Fact]
        public async Task NewTraceTestWithGenericType()
        {
            var sdk = GetSdk<StateExample>();

            Dictionary<string, object> data = new Dictionary<string, object>
            {
                ["weight"] = "123",
                ["valid"] = true,
                ["operators"] = new string[] { "1", "2" },
                ["operation"] = "my new operation 1"
            };

            SomeClass d = new SomeClass()
            {
                f11 = 1,
                f22 = data
            };

            NewTraceInput<SomeClass> input = new NewTraceInput<SomeClass>(ACTION_KEY, d);

            TraceState<StateExample, SomeClass> state = await sdk.NewTraceAsync<SomeClass>(input);
            someTraceState2 = state;

            Assert.NotNull(state.TraceId);
        }

        public class SomeClass
        {
            public int f11;
            public Dictionary<string, object> f22;
        }

        public class StateExample
        {
            public string f1;
            public SomeClass f2;
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
            string traceId = TRACE_ID;
            GetTraceStateInput input = new GetTraceStateInput(traceId);
            TraceState<object, object> state = await sdk.GetTraceStateAsync<object>(input);
            Assert.Equal(state.TraceId, traceId);
        }

        [Fact]
        public async Task GetTraceDetails()
        {
            var sdk = GetSdk();
            string traceId = TRACE_ID;

            GetTraceDetailsInput input = new GetTraceDetailsInput(traceId, 5, null, null, null);

            TraceDetails<object> details = await sdk.GetTraceDetailsAsync<object>(input);
            Debug.WriteLine(JsonHelper.ToJson(details));
            Assert.NotNull(details);
        }

        [Fact]
        public async Task GetIncomingTracesTest()
        {
            Sdk<object> sdk = GetSdk();
            PaginationInfo paginationInfo = new PaginationInfo(10, null, null, null);
            TracesState<object, object> state = await sdk.GetIncomingTracesAsync<object>(paginationInfo);
            Assert.NotNull(state);
        }

        [Fact]
        public async Task GetOutgoingTracesTest()
        {
            Sdk<object> sdk = GetSdk();
            PaginationInfo paginationInfo = new PaginationInfo(10, null, null, null);
            TracesState<object, object> state = await sdk.GetOutgoingTracesAsync<object>(paginationInfo);
            Assert.NotNull(state);
        }

        [Fact]
        public async Task GetAttestationTracesTest()
        {
            Sdk<object> sdk = GetSdk();
            PaginationInfo paginationInfo = new PaginationInfo(10, null, null, null);
            TracesState<object, object> state = await sdk.GetAttestationTracesAsync<object>(ACTION_KEY, paginationInfo);
            Assert.NotNull(state);
        }

        [Fact]
        public async Task GetBackLogTraceTest()
        {
            var sdk = GetSdk();
            PaginationInfo info = new PaginationInfo(2, null, null, null);
            var state = await sdk.GetBacklogTracesAsync<object>(info);
            Assert.NotNull(state);
        }

        //used to pass the trace from one test method to another
        private TraceState<object, object> someTraceState;
        [Fact]
        public async Task NewTraceTest()
        {
            var sdk = GetSdk();
            IDictionary<string, object> data = new Dictionary<string, object>
            {
                ["weight"] = "123",
                ["valid"] = true,
                ["operators"] = new string[] { "1", "2" },
                ["operation"] = "my new operation 1"
            };
            NewTraceInput<object> input = new NewTraceInput<object>(ACTION_KEY, data);

            TraceState<object, object> state = await sdk.NewTraceAsync<object>(input);
            someTraceState = state;

            Assert.NotNull(state.TraceId);
        }


        [Fact]
        public async Task AppendLinkTest()
        {
            await NewTraceTest();
            Assert.NotNull(someTraceState);
            Dictionary<string, object> data;
            string json = "{ operation: \"XYZ shipment departed port for ABC\"," + "    destination: \"ABC\", " + "    customsCheck: true, "
               + "    eta: \"2019-07-02T12:00:00.000Z\"" + "  }";
            data = JsonHelper.ObjectToMap(json);
            AppendLinkInput<object> appLinkInput = new AppendLinkInput<object>(ACTION_KEY, data, someTraceState.TraceId);
            TraceState<object, object> state = await GetSdk().AppendLinkAsync(appLinkInput);
            Assert.NotNull(state.TraceId);

        }


        [Fact]
        public async Task PushTraceTest()
        {
            await NewTraceTest();
            Assert.NotNull(someTraceState);

            PushTransferInput<object> push = new PushTransferInput<object>(someTraceState.TraceId, OTHER_GROUP, new object(), null);
            someTraceState = await GetSdk().PushTraceAsync<object>(push);

            Assert.NotNull(push.TraceId);
        }


        [Fact]
        [Obsolete("AcceptTransferTest is deprecated")]
        public async Task AcceptTransferTest()
        {
            await PushTraceTest();
            TransferResponseInput<Object> trInput = new TransferResponseInput<Object>(someTraceState.TraceId, null, null);
            TraceState<Object, Object> stateAccept = await GetOtherGroupSdk().AcceptTransferAsync(trInput);

            Assert.NotNull(stateAccept.TraceId);
        }

        [Fact]
        [Obsolete("RejectTransferTest is deprecated")]
        public async Task RejectTransferTest()
        {
            PaginationInfo paginationInfo = new PaginationInfo(10, null, null, null);
            TracesState<Object, Object> tracesIn = await GetSdk().GetIncomingTracesAsync<Object>(paginationInfo);

            string traceId = null;
            if (tracesIn.TotalCount == 0)
            {
                await PushTraceTest();
                traceId = someTraceState.TraceId;
            }
            else
            {
                someTraceState = tracesIn.Traces[0];
                traceId = someTraceState.TraceId;
            }
            TransferResponseInput<Object> trInput = new TransferResponseInput<Object>(traceId, null, null);
            TraceState<Object, Object> stateReject = await GetOtherGroupSdk().RejectTransferAsync(trInput);

            Assert.NotNull(stateReject.TraceId);
        }

        [Fact]
        [Obsolete("CancelTransferTest is deprecated")]
        public async Task CancelTransferTest()
        {
            await PushTraceTest();

            TransferResponseInput<Object> responseInput = new TransferResponseInput<Object>(someTraceState.TraceId, null, null);
            TraceState<Object, Object> statecancel = await GetSdk().CancelTransferAsync(responseInput);

            Assert.NotNull(statecancel.TraceId);
        }

        [Fact]
        public async Task NewTraceUploadTest()
        {
            Sdk<Object> sdk = GetSdk();

            IDictionary<string, object> data = new Dictionary<string, object>
            {
                ["weight"] = "123",
                ["valid"] = true,
                ["operators"] = new string[] { "1", "2" },
                ["operation"] = "my new operation 1"
            };

            data.Add("certificate1", FileWrapper.FromFilePath(GetTestFilePath()));
            data.Add("certificates", new Identifiable[] { FileWrapper.FromFilePath(GetTestFilePath()) });

            NewTraceInput<Object> newTraceInput = new NewTraceInput<Object>(ACTION_KEY, data);

            TraceState<object, object> state = await sdk.NewTraceAsync<object>(newTraceInput);
            Assert.NotNull(state.TraceId);
            someTraceState = state;
        }

        [Fact]
        public async Task UploadFilesInLinkDataTest()
        {
            Sdk<Object> sdk = GetSdk();
            IDictionary<string, object> data = new Dictionary<string, object>
            {
                ["weight"] = "123",
                ["valid"] = true,
                ["operators"] = new string[] { "1", "2" },
                ["operation"] = "my new operation 1"
            };

            data.Add("Certificate1", FileWrapper.FromFilePath(GetTestFilePath()));
            data.Add("Certificates", new Identifiable[] { FileWrapper.FromFilePath(GetTestFilePath()) });

            await sdk.UploadFilesInLinkData(data);

            Assert.True(FileRecord.IsFileRecord(data["Certificate1"]));
            Assert.True(FileRecord.IsFileRecord(((object[])data["Certificates"])[0]));

            Debug.WriteLine(JsonHelper.ToJson(data));
        }


        [Fact]
        public async Task downloadFilesInObjectTest()
        {
            TraceState<Object, Object> state;
            if (someTraceState == null)
            {
                await NewTraceUploadTest();
            }
            state = someTraceState;

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
        public async Task traceTagsRWTest()
        {
            Sdk<object> sdk = GetSdk();

            // Add a tag to a trace
            string traceId = TRACE_ID;
            Guid uuid = System.Guid.NewGuid();
            string randomUUIDString = uuid.ToString();
            AddTagsToTraceInput input = new AddTagsToTraceInput(traceId, new string[] { randomUUIDString });

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
    }
}
