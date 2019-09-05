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

namespace SDKTest
{

    [TestClass]
    public class SdkTest
    {

        private const string FORM_ID = "8209";
        private const string WORFKLOW_ID = "591";

        private const String MY_GROUP = "1744";



        public Sdk<T> GetSdk<T>()
        {
            var pem = "-----BEGIN ED25519 PRIVATE KEY-----\nMFACAQAwBwYDK2VwBQAEQgRACaNT4cup/ZQAq4IULZCrlPB7eR1QTCN9V3Qzct8S\nYp57BqN4FipIrGpyclvbT1FKQfYLJpeBXeCi2OrrQMTgiw==\n-----END ED25519 PRIVATE KEY-----\n";
            var workflowId = "591";
            Secret s = Secret.NewPrivateKeySecret(pem);
            SdkOptions opts = new SdkOptions(workflowId, s);
            Sdk<T> sdk = new Sdk<T>(opts);

            return sdk;
        }

        public Sdk<object> GetSdk()
        {
            var pem = "-----BEGIN ED25519 PRIVATE KEY-----\nMFACAQAwBwYDK2VwBQAEQgRACaNT4cup/ZQAq4IULZCrlPB7eR1QTCN9V3Qzct8S\nYp57BqN4FipIrGpyclvbT1FKQfYLJpeBXeCi2OrrQMTgiw==\n-----END ED25519 PRIVATE KEY-----\n";
            var workflowId = "591";
            Secret s = Secret.NewPrivateKeySecret(pem);
            SdkOptions opts = new SdkOptions(workflowId, s);
            Sdk<object> sdk = new Sdk<object>(opts);

            return sdk;
        }
        [TestMethod]
        public async Task LoginWtihPrivateKeyDemo()
        {
            var sdk = GetSdk();
            string token = await sdk.LoginAsync();
            Debug.WriteLine(token); // write to output window
            Console.WriteLine(token); // write to console window
        }


        //used to pass the trace from one test method to another
        private TraceState<StateExample, SomeClass> someTraceState2;
        [TestMethod]
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

            NewTraceInput<SomeClass> input = new NewTraceInput<SomeClass>(FORM_ID, d);

            TraceState<StateExample, SomeClass> state = await sdk.NewTraceAsync<SomeClass>(input);
            someTraceState2 = state;



            Assert.IsNotNull(state.TraceId);
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

        [TestMethod]
        public async Task LoginWithCredentialsDemo()
        {
            var sdk = GetSdk();
            string token = await sdk.LoginAsync();
            Debug.WriteLine(token); // write to output window
            Console.WriteLine(token); // write to console window
        }

        [TestMethod]
        public async Task TestGetConfigAsync()
        {
            var sdk = GetSdk();

            SdkConfig config = await sdk.GetConfigAsync();

        }

        [TestMethod]
        public async Task GetTraceStateTest()
        {

            Sdk<object> sdk = GetSdk();
            string traceId = "a41257f9-2d9d-4d42-ab2a-fd0c83ea31df";
            GetTraceStateInput input = new GetTraceStateInput(traceId);
            TraceState<object, object> state = await sdk.GetTraceStateAsync<object>(input);
            Assert.AreEqual(state.TraceId, traceId);
        }

        [TestMethod]
        public async Task GetTraceDetails()
        {
            var sdk = GetSdk();
            string traceId = "a41257f9-2d9d-4d42-ab2a-fd0c83ea31df";

            GetTraceDetailsInput input = new GetTraceDetailsInput(traceId, 5, null, null, null);

            TraceDetails<object> details = await sdk.GetTraceDetailsAsync<object>(input);
            Console.WriteLine(JsonHelper.ToJson(details));


        }

        [TestMethod]
        public async Task GetIncomingTracesTest()
        {

            Sdk<object> sdk = GetSdk();
            PaginationInfo paginationInfo = new PaginationInfo(10, null, null, null);
            TracesState<object, object> state = await sdk.GetIncomingTracesAsync<object>(paginationInfo);
        }

        [TestMethod]
        public async Task GetOutgoingTracesTest()
        {

            Sdk<object> sdk = GetSdk();
            PaginationInfo paginationInfo = new PaginationInfo(10, null, null, null);
            TracesState<object, object> state = await sdk.GetOutgoingTracesAsync<object>(paginationInfo);
        }

        [TestMethod]
        public async Task GetAttestationTracesTest()
        {

            Sdk<object> sdk = GetSdk();
            PaginationInfo paginationInfo = new PaginationInfo(10, null, null, null);
            TracesState<object, object> state = await sdk.GetAttestationTracesAsync<object>(FORM_ID, paginationInfo);
        }

        [TestMethod]
        public async Task GetBackLogTraceTest()
        {

            var sdk = GetSdk();

            PaginationInfo info = new PaginationInfo(2, null, null, null);

            await sdk.GetBacklogTracesAsync<object>(info);
        }


        //used to pass the trace from one test method to another
        private TraceState<object, object> someTraceState;
        [TestMethod]
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
            NewTraceInput<object> input = new NewTraceInput<object>(FORM_ID, data);

            TraceState<object, object> state = await sdk.NewTraceAsync<object>(input);
            someTraceState = state;

            Assert.IsNotNull(state.TraceId);
        }


        [TestMethod]
        public async Task AppendLinkTest()
        {

            await NewTraceTest();
            Assert.IsNotNull(someTraceState);
            Dictionary<string, object> data;
            string json = "{ operation: \"XYZ shipment departed port for ABC\"," + "    destination: \"ABC\", " + "    customsCheck: true, "
               + "    eta: \"2019-07-02T12:00:00.000Z\"" + "  }";
            data = JsonHelper.ObjectToMap(json);
            AppendLinkInput<object> appLinkInput = new AppendLinkInput<object>(FORM_ID, data, someTraceState.TraceId);
            TraceState<object, object> state = await GetSdk().AppendLinkAsync(appLinkInput);
            Assert.IsNotNull(state.TraceId);

        }


        [TestMethod]
        public async Task PushTraceTest()
        {
            await NewTraceTest();
            Assert.IsNotNull(someTraceState);
            IDictionary<string, object> data = new Dictionary<string, object>() { { "why", "because im testing the pushTrace 2" } };

            PushTransferInput<object> push = new PushTransferInput<object>(someTraceState.TraceId, "86", data, null);
            someTraceState = await GetSdk().PushTraceAsync<object>(push);

            Assert.IsNotNull(push.TraceId);

        }


        [TestMethod]
        public async Task PushTraceToMyGroupTest()
        {
            await NewTraceTest();
            Assert.IsNotNull(someTraceState);
            IDictionary<string, object> data = new Dictionary<string, object>() { { "why", "because im testing the pushTrace 2" } };

            PushTransferInput<object> push = new PushTransferInput<object>(someTraceState.TraceId, MY_GROUP, data, null);
            someTraceState = await GetSdk().PushTraceAsync<object>(push);

            Assert.IsNotNull(push.TraceId);

        }


        [TestMethod]
        public async Task PullTraceTest()
        {

            await RejectTransferTest();

            IDictionary<string, string> data = new Dictionary<string, string>() { { "why", "because im testing the pushTrace 2" } };

            PullTransferInput<object> pull = new PullTransferInput<object>(someTraceState.TraceId, data, null);
            TraceState<object, object> statepul = await GetSdk().PullTraceAsync(pull);

            Assert.IsNotNull(statepul.TraceId);

        }


        [TestMethod]
        public async Task AcceptTransferTest()
        {

            await PushTraceToMyGroupTest();
            TransferResponseInput<Object> trInput = new TransferResponseInput<Object>(someTraceState.TraceId, null, null);
            TraceState<Object, Object> stateAccept = await GetSdk().AcceptTransferAsync(trInput);

            Assert.IsNotNull(stateAccept.TraceId);
        }



        [TestMethod]
        public async Task RejectTransferTest()
        {
            PaginationInfo paginationInfo = new PaginationInfo(10, null, null, null);
            TracesState<Object, Object> tracesIn = await GetSdk().GetIncomingTracesAsync<Object>(paginationInfo);

            string traceId = null;
            if (tracesIn.TotalCount == 0)
            {
                await PushTraceToMyGroupTest();
                traceId = someTraceState.TraceId;
            }
            else
            {
                someTraceState = tracesIn.Traces[0];
                traceId = someTraceState.TraceId;
            }
            TransferResponseInput<Object> trInput = new TransferResponseInput<Object>(traceId, null, null);
            TraceState<Object, Object> stateReject = await GetSdk().RejectTransferAsync(trInput);

            Assert.IsNotNull(stateReject.TraceId);


        }



        [TestMethod]
        public async Task CancelTransferTest()
        {

            await PushTraceTest();

            TransferResponseInput<Object> responseInput = new TransferResponseInput<Object>(someTraceState.TraceId, null, null);
            TraceState<Object, Object> statecancel = await GetSdk().CancelTransferAsync(responseInput);

            Assert.IsNotNull(statecancel.TraceId); ;

        }



        [TestMethod]
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

            data.Add("Certificate1",FileWrapper.FromFilePath(Path.GetFullPath("../../Resources/TestFile1.txt")));
            data.Add("Certificates", new Identifiable[] { FileWrapper.FromFilePath(Path.GetFullPath("../../Resources/TestFile1.txt")) });

            NewTraceInput<Object> newTraceInput = new NewTraceInput<Object>(FORM_ID, data);

            TraceState<object, object> state = await sdk.NewTraceAsync<object>(newTraceInput);
            Assert.IsNotNull(state.TraceId);
            someTraceState = state;

        }


        [TestMethod]
        public async Task downloadFilesInObjectTest()
        {

            TraceState<Object, Object> state;
            try
            {
                state = await GetSdk().GetTraceStateAsync<object>(new GetTraceStateInput("dee0dd04-5d58-4c4e-a72d-a759e37ae337"));
            }
            catch (Exception e)
            {  //trace not found
                await NewTraceUploadTest();
                state = someTraceState;
            }

            Object dataWithRecords = state.HeadLink.FormData();

            object dataWithFiles = await GetSdk().DownloadFilesInObject(dataWithRecords);
            IDictionary<String, Property<FileWrapper>> fileWrappers = Helpers.ExtractFileWrappers(dataWithFiles);

            foreach (Property<FileWrapper> fileWrapperProp in fileWrappers.Values)
            {
                WriteFileToDisk(fileWrapperProp.Value);
                //assert files are equal
            }


        }
        private void WriteFileToDisk(FileWrapper fWrapper)
        {

            MemoryStream buffer = fWrapper.DecrytptedData();

            FileInfo file = new FileInfo(Path.GetFullPath("./Resources/out/" + fWrapper.Info().Name));


            if (!Directory.Exists(file.DirectoryName))
            {
                Directory.CreateDirectory(file.DirectoryName);
            }
            //    if (File.Exists(file.FullName))
            //        try
            //        {
            //            File.Create(file.FullName);
            //        }
            //        catch (IOException e1)
            //        {
            //            throw new TraceSdkException("Failed to create output file");
            //        }
            //}

            using (FileStream fs = new FileStream(file.FullName, FileMode.Create, System.IO.FileAccess.Write))
            {
                byte[] bytes = new byte[buffer.Length];
                buffer.Read(bytes, 0, (int)buffer.Length);
                fs.Write(bytes, 0, bytes.Length);
                buffer.Close();
            }

        }
    }
}