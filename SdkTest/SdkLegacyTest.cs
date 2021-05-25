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

namespace SdkTest
{
    public class SdkLegacyTest
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
            opts.GroupLabel = MY_GROUP_LABEL;
            Sdk<object> sdk = new Sdk<object>(opts);

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
            opts.GroupLabel = MY_GROUP_LABEL;
            Sdk<object> sdk = new Sdk<object>(opts);

            return sdk;
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

    }
}
