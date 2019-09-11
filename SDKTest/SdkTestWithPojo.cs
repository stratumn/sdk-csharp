using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Stratumn.Sdk;
using Stratumn.Sdk.Model.Client;
using Stratumn.Sdk.Model.Sdk;
using Stratumn.Sdk.Model.Trace;
using System.Diagnostics;
using System.Collections.Generic;
using Stratumn.Chainscript.utils;
using System.IO;
using Stratumn.Sdk.Model.Misc;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Stratumn.SDKTest
{

    [TestClass]
    public class SdkTestPojoTest
    {

        private const string FORM_ID = "8209";
        private const string WORFKLOW_ID = "591";

        private const String MY_GROUP = "1744";

        //used to pass the trace from one test method to another
        private TraceState<StateExample, DataClass> someTraceState;
        public Sdk<T> GetSdk<T>()
        {
            var pem = "-----BEGIN ED25519 PRIVATE KEY-----\nMFACAQAwBwYDK2VwBQAEQgRACaNT4cup/ZQAq4IULZCrlPB7eR1QTCN9V3Qzct8S\nYp57BqN4FipIrGpyclvbT1FKQfYLJpeBXeCi2OrrQMTgiw==\n-----END ED25519 PRIVATE KEY-----\n";
            var workflowId = "591";
            Secret s = Secret.NewPrivateKeySecret(pem);
            SdkOptions opts = new SdkOptions(workflowId, s);
            opts.Endpoints = new Endpoints
            {
                Trace = "https://trace-api.staging.stratumn.com",
                Account = "https://account-api.staging.stratumn.com",
                Media = "https://media-api.staging.stratumn.com",
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

        [TestMethod]
        public async Task GetTraceStateTestWithPojo()
        {

            Sdk<StateExample> sdk = GetSdk<StateExample>();
            string traceId = "5ef3faed-9445-434e-acd5-a20cfa0284b1";
            GetTraceStateInput input = new GetTraceStateInput(traceId);
            TraceState<StateExample, HeadLinkData> state = await sdk.GetTraceStateAsync<HeadLinkData>(input);
            Assert.AreEqual(state.TraceId, traceId);
        }

        [TestMethod]
        public async Task GetTraceDetailsWithPojo()
        {
            Sdk<StateExample> sdk = GetSdk<StateExample>();
            string traceId = "5ef3faed-9445-434e-acd5-a20cfa0284b1";

            GetTraceDetailsInput input = new GetTraceDetailsInput(traceId, 5, null, null, null);

            TraceDetails<HeadLinkData> details = await sdk.GetTraceDetailsAsync<HeadLinkData>(input);
            Console.WriteLine(JsonHelper.ToJson(details));

        }

        [TestMethod]
        public async Task GetIncomingTracesTestWithPojo()
        {
            Sdk<StateExample> sdk = GetSdk<StateExample>();
            PaginationInfo paginationInfo = new PaginationInfo(10, null, null, null);
            TracesState<StateExample, HeadLinkData> state = await sdk.GetIncomingTracesAsync<HeadLinkData>(paginationInfo);
            Console.WriteLine(JsonHelper.ToJson(state));
        }

        [TestMethod]
        public async Task GetOutoingTrafesTestWithPojo()
        {

            Sdk<StateExample> sdk = GetSdk<StateExample>();
            PaginationInfo paginationInfo = new PaginationInfo(10, null, null, null);
            TracesState<StateExample, HeadLinkData> state = await sdk.GetOutgoingTracesAsync<HeadLinkData>(paginationInfo);
            Console.WriteLine(JsonHelper.ToJson(state));
        }

        [TestMethod]
        public async Task GetAttestationTracesTestWithPojo()
        {

            Sdk<StateExample> sdk = GetSdk<StateExample>();
            PaginationInfo paginationInfo = new PaginationInfo(10, null, null, null);
            TracesState<StateExample, HeadLinkData> state = await sdk.GetAttestationTracesAsync<HeadLinkData>(FORM_ID, paginationInfo);
            Console.WriteLine(JsonHelper.ToJson(state));
        }

        [TestMethod]
        public async Task GetBackLogTraceTestWithPojo()
        {

            Sdk<StateExample> sdk = GetSdk<StateExample>();

            PaginationInfo info = new PaginationInfo(2, null, null, null);

            var backlog = await sdk.GetBacklogTracesAsync<object>(info);
            Console.WriteLine(JsonHelper.ToJson(backlog));
        }


        //used to pass the trace from one test method to another
        private TraceState<StateExample, DataClass> someTraceState2;

        [TestMethod]
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

            NewTraceInput<DataClass> input = new NewTraceInput<DataClass>(FORM_ID, d);

            TraceState<StateExample, DataClass> state = await sdk.NewTraceAsync<DataClass>(input);
            someTraceState = state;



            Assert.IsNotNull(state.TraceId);
        }




        [TestMethod]
        public async Task AppendLinkTestWithPojo()
        {

            var sdk = GetSdk<StateExample>();
            await NewTraceTestWithPojo();
            Assert.IsNotNull(someTraceState);
            Dictionary<string, object> data;
            string json = "{ operation: \"XYZ shipment departed port for ABC\"," + "    destination: \"ABC\", " + "    customsCheck: true, "
               + "    eta: \"2019-07-02T12:00:00.000Z\"" + "  }";

            data = JsonHelper.ObjectToMap(json);

            DataClass d = new DataClass()
            {
                f11 = 1,
                f22 = data
            };


            AppendLinkInput<DataClass> appLinkInput = new AppendLinkInput<DataClass>(FORM_ID, d, someTraceState.TraceId);
            TraceState<StateExample, DataClass> state = await sdk.AppendLinkAsync<DataClass>(appLinkInput);
            Assert.IsNotNull(state.TraceId);

        }


        [TestMethod]
        public async Task PushTraceTestWithPojo()
        {
            var sdk = GetSdk<StateExample>();

            await NewTraceTestWithPojo();
            Assert.IsNotNull(someTraceState);
            Dictionary<string, object> data = new Dictionary<string, object>() { { "why", "because im testing the pushTrace 2" } };

            DataClass d = new DataClass()
            {
                f11 = 1,
                f22 = data
            };


            PushTransferInput<DataClass> push = new PushTransferInput<DataClass>(someTraceState.TraceId, "86", d, null);
            someTraceState = await sdk.PushTraceAsync<DataClass>(push);

            Assert.IsNotNull(push.TraceId);

        }


        [TestMethod]
        public async Task PushTraceToMyGroupTestWithPojo()
        {
            await NewTraceTestWithPojo();
            Assert.IsNotNull(someTraceState);
            Dictionary<string, object> data = new Dictionary<string, object>() { { "why", "because im testing the pushTrace 2" } };
            DataClass d = new DataClass()
            {
                f11 = 1,
                f22 = data
            };


            PushTransferInput<DataClass> push = new PushTransferInput<DataClass>(someTraceState.TraceId, MY_GROUP, d, null);
            someTraceState = await GetSdk<StateExample>().PushTraceAsync<DataClass>(push);

            Assert.IsNotNull(push.TraceId);

        }


        [TestMethod]
        public async Task PullTraceTestWithPojo()
        {

            await RejectTransferTestWithPojo();

            Dictionary<string, object> data = new Dictionary<string, object>() { { "why", "because im testing the pushTrace 2" } };
            DataClass d = new DataClass()
            {
                f11 = 1,
                f22 = data
            };


            PullTransferInput<DataClass> pull = new PullTransferInput<DataClass>(someTraceState.TraceId, d, null);
            TraceState<StateExample, DataClass> statepul = await GetSdk<StateExample>().PullTraceAsync(pull);

            Assert.IsNotNull(statepul.TraceId);

        }


        [TestMethod]
        public async Task AcceptTransferTestWithPojo()
        {

            await PushTraceToMyGroupTestWithPojo();
            Dictionary<string, object> data = new Dictionary<string, object>() { { "why", "because im testing the pushTrace 2" } };

            DataClass d = new DataClass()
            {
                f11 = 1,
                f22 = data
            };

            TransferResponseInput<DataClass> trInput = new TransferResponseInput<DataClass>(someTraceState.TraceId, d, null);
            TraceState<StateExample, DataClass> stateAccept = await GetSdk<StateExample>().AcceptTransferAsync<DataClass>(trInput);

            Assert.IsNotNull(stateAccept.TraceId);
        }



        [TestMethod]
        public async Task RejectTransferTestWithPojo()
        {

            PaginationInfo paginationInfo = new PaginationInfo(10, null, null, null);

            var sdk = GetSdk<StateExample>();

            TracesState<StateExample, DataClass> tracesIn = await sdk.GetIncomingTracesAsync<DataClass>(paginationInfo);

            string traceId = null;
            if (tracesIn.TotalCount == 0)
            {
                await PushTraceToMyGroupTestWithPojo();
                traceId = someTraceState.TraceId;
            }
            else
            {
                someTraceState = tracesIn.Traces[0];
                traceId = someTraceState.TraceId;
            }
            TransferResponseInput<DataClass> trInput = new TransferResponseInput<DataClass>(traceId, null, null);
            TraceState<StateExample, DataClass> stateReject = await sdk.RejectTransferAsync<DataClass>(trInput);

            Assert.IsNotNull(stateReject.TraceId);


        }


        [TestMethod]
        public async Task CancelTransferTestWithPojo()
        {

            Dictionary<string, object> data = new Dictionary<string, object>() { { "why", "because im testing the pushTrace 2" } };

            DataClass d = new DataClass()
            {
                f11 = 1,
                f22 = data
            };

            await PushTraceTestWithPojo();

            TransferResponseInput<DataClass> responseInput = new TransferResponseInput<DataClass>(someTraceState.TraceId, d, null);
            TraceState<StateExample, DataClass> statecancel = await GetSdk<StateExample>().CancelTransferAsync< DataClass>(responseInput);

            Assert.IsNotNull(statecancel.TraceId); ;

        }


        public class DataClass
        {

            public int f11;
            public Dictionary<string, object> f22;
        }


        public class SomeClass2
        {


            public Dictionary<string, string> f22;
        }


        public class StateExample
        {

            public string f1;
            public DataClass f2;
        }


        public class Step
        {

            public StepData data;


        }

        public class StepData
        {
          
            public Identifiable[] stp_form_section;
        }


        [TestMethod]
        public async Task NewTraceUploadInClassTest()
        {

            Sdk<Object> sdk = GetSdk<Object>();

            Step s = new Step();
            s.data = new StepData();
            s.data.stp_form_section =new Identifiable[] { FileWrapper.FromFilePath(Path.GetFullPath("../../Resources/TestFile1.txt")) };


            NewTraceInput<Step> newTraceInput = new NewTraceInput<Step>(FORM_ID, s);

            TraceState<object, Step> state = await sdk.NewTraceAsync<Step>(newTraceInput);
            Assert.IsNotNull(state.TraceId);
            Debug.WriteLine(state.TraceId);
        }
    }
}