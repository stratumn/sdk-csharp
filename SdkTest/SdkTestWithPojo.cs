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
        private const string WORKFLOW_ID = "591";
        private const string ACTION_KEY = "action1";

        private const String MY_GROUP = "1744";
        private const String PEM_PRIVATEKEY = "-----BEGIN ED25519 PRIVATE KEY-----\nMFACAQAwBwYDK2VwBQAEQgRACaNT4cup/ZQAq4IULZCrlPB7eR1QTCN9V3Qzct8S\nYp57BqN4FipIrGpyclvbT1FKQfYLJpeBXeCi2OrrQMTgiw==\n-----END ED25519 PRIVATE KEY-----\n";

        private static String PEM_PRIVATEKEY_2 = "-----BEGIN ED25519 PRIVATE KEY-----\nMFACAQAwBwYDK2VwBQAEQgRAWotrb1jJokHr7AVQTS6f6W7dFYnKpVy+DV++sG6x\nlExB4rtrKpCAEPt5q7oT6/lcF4brFSNiCxLPnHqiSjcyVw==\n-----END ED25519 PRIVATE KEY-----\n";
        private static String OTHER_GROUP = "1785";

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

        [Fact]
        public async Task GetTraceStateTestWithPojo()
        {
            Sdk<StateExample> sdk = GetSdk<StateExample>();
            string traceId = "191516ec-5f8c-4757-9061-8c7ab06cf0a0";
            GetTraceStateInput input = new GetTraceStateInput(traceId);
            TraceState<StateExample, HeadLinkData> state = await sdk.GetTraceStateAsync<HeadLinkData>(input);
            Assert.Equal(state.TraceId, traceId);
        }

        [Fact]
        public async Task GetTraceDetailsWithPojo()
        {
            Sdk<StateExample> sdk = GetSdk<StateExample>();
            string traceId = "191516ec-5f8c-4757-9061-8c7ab06cf0a0";

            GetTraceDetailsInput input = new GetTraceDetailsInput(traceId, 5, null, null, null);

            TraceDetails<HeadLinkData> details = await sdk.GetTraceDetailsAsync<HeadLinkData>(input);
            Debug.WriteLine(JsonHelper.ToJson(details));
            Assert.NotNull(details);
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
        public async Task AppendLinkTestWithPojo()
        {
            var sdk = GetSdk<StateExample>();
            await NewTraceTestWithPojo();
            Assert.NotNull(someTraceState);
            Dictionary<string, object> data;
            string json = "{ operation: \"XYZ shipment departed port for ABC\"," + "    destination: \"ABC\", " + "    customsCheck: true, "
               + "    eta: \"2019-07-02T12:00:00.000Z\"" + "  }";

            data = JsonHelper.ObjectToMap(json);

            DataClass d = new DataClass()
            {
                f11 = 1,
                f22 = data
            };


            AppendLinkInput<DataClass> appLinkInput = new AppendLinkInput<DataClass>(ACTION_KEY, d, someTraceState.TraceId);
            TraceState<StateExample, DataClass> state = await sdk.AppendLinkAsync<DataClass>(appLinkInput);
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

        public class Step
        {
            public Identifiable[] stp_form_section;
        }

        [Fact]
        public async Task NewTraceUploadInClassTest()
        {
            Sdk<Object> sdk = GetSdk<Object>();

            Step s = new Step();
            s.stp_form_section = new Identifiable[] { FileWrapper.FromFilePath(GetTestFilePath()) };

            NewTraceInput<Step> newTraceInput = new NewTraceInput<Step>(ACTION_KEY, s);

            TraceState<object, Step> state = await sdk.NewTraceAsync<Step>(newTraceInput);
            Assert.NotNull(state.TraceId);
            Debug.WriteLine(state.TraceId);
        }
    }
}