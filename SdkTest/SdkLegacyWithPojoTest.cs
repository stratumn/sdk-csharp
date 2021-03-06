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

namespace SdkTest
{
    public class SdkLegacyWithPojoTest
    {
        private const string WORKFLOW_ID = "591";
        private const string ACTION_KEY = "action1";
        private const string COMMENT_ACTION_KEY = "3HflvBg1mU";
        private const string MY_GROUP = "1744";
        private const string MY_GROUP_LABEL = "group1";

        private const string TRACE_ID = "191516ec-5f8c-4757-9061-8c7ab06cf0a0";

        private const string PEM_PRIVATEKEY = "-----BEGIN ED25519 PRIVATE KEY-----\nMFACAQAwBwYDK2VwBQAEQgRAjgtjpc1iOR4zYm+21McRGoWr0WM1NBkm26uZmFAx\n853QZ8CRL/HWGCPpEt18JrHZr9ZwA9UyoEosPR8gPakZFQ==\n-----END ED25519 PRIVATE KEY-----\n";

        private const string PEM_PRIVATEKEY_2 = "-----BEGIN ED25519 PRIVATE KEY-----\nMFACAQAwBwYDK2VwBQAEQgRArbo87/1Yd/nOqFwmmcuxm01T9/pqkeARQxK9y4iG\nF3Xe1W+/2UOr/rYuQPFHQC4a/F0r6nVJGgCI1Ghc/luHZw==\n-----END ED25519 PRIVATE KEY-----\n";
        private const string OTHER_GROUP = "1785";
        private const string OTHER_GROUP_LABEL = "stp";

        private const string TRACE_URL = "https://trace-api.staging.stratumn.com";
        private const string ACCOUNT_URL = "https://account-api.staging.stratumn.com";
        private const string MEDIA_URL = "https://media-api.staging.stratumn.com";

        // used to pass the trace from one test method to another
        private TraceState<StateExample, DataClass> someTraceState;

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
            opts.GroupLabel = MY_GROUP_LABEL;
            Sdk<T> sdk = new Sdk<T>(opts);

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
            opts.GroupLabel = OTHER_GROUP_LABEL;
            Sdk<T> sdk = new Sdk<T>(opts);

            return sdk;
        }

        public class HeadLinkData
        {
            public bool Valid { get; set; }
            public string Weight { get; set; }
            public string Operation { get; set; }
            public String[] Operators;
        }

        public class DataClass
        {
            public int f11;
            public Dictionary<string, object> f22;
        }

        public class StateExample
        {
            public string f1;
            public DataClass f2;
        }


        [Fact]
        public async Task GetIncomingTracesTestWithPojo()
        {
            Sdk<StateExample> sdk = GetSdk<StateExample>();
            PaginationInfo paginationInfo = new PaginationInfo(10, null, null, null);
            TracesState<StateExample, HeadLinkData> state = await sdk.GetIncomingTracesAsync<HeadLinkData>(paginationInfo);
            Debug.WriteLine(JsonHelper.ToJson(state));
            Assert.NotNull(state);
        }

        [Fact]
        public async Task GetOutoingTrafesTestWithPojo()
        {
            Sdk<StateExample> sdk = GetSdk<StateExample>();
            PaginationInfo paginationInfo = new PaginationInfo(10, null, null, null);
            TracesState<StateExample, HeadLinkData> state = await sdk.GetOutgoingTracesAsync<HeadLinkData>(paginationInfo);
            Debug.WriteLine(JsonHelper.ToJson(state));
            Assert.NotNull(state);
        }

        [Fact]
        public async Task GetAttestationTracesTestWithPojo()
        {
            Sdk<StateExample> sdk = GetSdk<StateExample>();
            PaginationInfo paginationInfo = new PaginationInfo(10, null, null, null);
            TracesState<StateExample, HeadLinkData> state = await sdk.GetAttestationTracesAsync<HeadLinkData>(ACTION_KEY, paginationInfo);
            Debug.WriteLine(JsonHelper.ToJson(state));
            Assert.NotNull(state);
        }

        [Fact]
        public async Task GetBackLogTraceTestWithPojo()
        {
            Sdk<StateExample> sdk = GetSdk<StateExample>();
            PaginationInfo info = new PaginationInfo(2, null, null, null);

            var backlog = await sdk.GetBacklogTracesAsync<object>(info);
            Debug.WriteLine(JsonHelper.ToJson(backlog));
            Assert.NotNull(backlog);
        }

        [Fact]
        public async Task NewTraceTestWithPojo()
        {
            var sdk = GetSdk<StateExample>();

            Dictionary<string, object> data = new Dictionary<string, object>
            {
                ["weight"] = "123",
                ["valid"] = true,
                ["operators"] = new string[] { "1", "2" },
                ["operation"] = "my new operation 1"
            };

            DataClass d = new DataClass()
            {
                f11 = 1,
                f22 = data
            };

            NewTraceInput<DataClass> input = new NewTraceInput<DataClass>(ACTION_KEY, d);

            TraceState<StateExample, DataClass> state = await sdk.NewTraceAsync<DataClass>(input);
            someTraceState = state;

            Assert.NotNull(state.TraceId);
        }

        [Fact]
        public async Task PushTraceTestWithPojo()
        {
            var sdk = GetSdk<StateExample>();

            await NewTraceTestWithPojo();
            Assert.NotNull(someTraceState);

            PushTransferInput<Object> push = new PushTransferInput<Object>(someTraceState.TraceId, OTHER_GROUP, new Object(), null);
            await sdk.PushTraceAsync<Object>(push);

            Assert.NotNull(push.TraceId);
        }

        [Fact]
        [Obsolete("RejectTransferTestWithPojo is deprecated")]
        public async Task AcceptTransferTestWithPojo()
        {
            await PushTraceTestWithPojo();

            TransferResponseInput<Object> trInput = new TransferResponseInput<Object>(someTraceState.TraceId, new Object(), null);
            TraceState<StateExample, Object> stateAccept = await GetOtherGroupSdk<StateExample>().AcceptTransferAsync<Object>(trInput);

            Assert.NotNull(stateAccept.TraceId);
        }

        [Fact]
        [Obsolete("RejectTransferTestWithPojo is deprecated")]
        public async Task RejectTransferTestWithPojo()
        {
            PaginationInfo paginationInfo = new PaginationInfo(10, null, null, null);

            var sdk = GetSdk<StateExample>();

            TracesState<StateExample, DataClass> tracesIn = await sdk.GetIncomingTracesAsync<DataClass>(paginationInfo);

            string traceId = null;
            if (tracesIn.TotalCount == 0)
            {
                await PushTraceTestWithPojo();
                traceId = someTraceState.TraceId;
            }
            else
            {
                someTraceState = tracesIn.Traces[0];
                traceId = someTraceState.TraceId;
            }
            TransferResponseInput<DataClass> trInput = new TransferResponseInput<DataClass>(traceId, null, null);
            TraceState<StateExample, DataClass> stateReject = await GetOtherGroupSdk<StateExample>().RejectTransferAsync<DataClass>(trInput);

            Assert.NotNull(stateReject.TraceId);
        }

        [Fact]
        [Obsolete("CancelTransferTestWithPojo is deprecated")]
        public async Task CancelTransferTestWithPojo()
        {

            await PushTraceTestWithPojo();

            TransferResponseInput<Object> responseInput = new TransferResponseInput<Object>(someTraceState.TraceId, new Object(), null);
            TraceState<StateExample, Object> statecancel = await GetSdk<StateExample>().CancelTransferAsync<Object>(responseInput);

            Assert.NotNull(statecancel.TraceId);
        }
    }
}
