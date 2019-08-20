using stratumn.sdk;
using stratumn.sdk.Model.Clients;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CNP
{
    class Program
    {
        static void  Main(string[] args)
        {

            LoginWtihPrivateKeyDemo().Wait();

            LoginWithCredentialsDemo().Wait();
        }

        private static async Task LoginWtihPrivateKeyDemo()
        {
            var pem = "-----BEGIN ED25519 PRIVATE KEY-----\nMFACAQAwBwYDK2VwBQAEQgRACaNT4cup/ZQAq4IULZCrlPB7eR1QTCN9V3Qzct8S\nYp57BqN4FipIrGpyclvbT1FKQfYLJpeBXeCi2OrrQMTgiw==\n-----END ED25519 PRIVATE KEY-----\n";
            var workflowId = "565";
            Secret s = Secret.NewPrivateKeySecret(pem);
            SdkOptions opts = new SdkOptions(workflowId, s);
            Sdk sdk = new Sdk(opts);
            String token = await sdk.LoginAsync();
            Debug.WriteLine(token); // write to output window
            Console.WriteLine(token); // write to console window
        }

        private static async Task LoginWithCredentialsDemo()
        {

            var workflowId = "565";
            Secret s = Secret.NewCredentialSecret("dev@sdk", "test-test");
            SdkOptions opts = new SdkOptions(workflowId, s);
            Sdk sdk = new Sdk(opts);
            String token = await sdk.LoginAsync();
            Debug.WriteLine(token); // write to output window
            Console.WriteLine(token); // write to console window
        }
    }
}
