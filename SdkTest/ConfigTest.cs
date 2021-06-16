using dotenv.net;
using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace SdkTest
{
    public class ConfigTest
    {
        public string WORKFLOW_ID;
        public string MY_GROUP;
        public string TRACE_ID;
        public string OTHER_GROUP;
        public string ACCOUNT_API_URL;
        public string TRACE_API_URL;
        public string MEDIA_API_URL;

        // Non env dependant
        public string COMMENT_ACTION_KEY = "comment";
        public string INIT_ACTION_KEY = "init";
        public string UPLOAD_DOCUMENTS_ACTION_KEY = "uploadDocuments";
        public string IMPORT_TA_ACTION_KEY = "importTa";
        public string MY_GROUP_LABEL = "group1";
        public string OTHER_GROUP_LABEL = "group2";
        public string OTHER_GROUP_NAME = "SDKs Group 2";

        public string PEM_PRIVATEKEY_2;
        public string PEM_PRIVATEKEY;

        private string GetEnvFilePath(
                    // This is a weird hack to get the location of this source file
                    // https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.callerfilepathattribute?view=netstandard-2.0
                    [CallerFilePath] string callerFilePath = ""
                )
        {
            return Path.Combine(
                Directory.GetParent(callerFilePath).FullName,
                ".env"
            );
        }

        public ConfigTest()
        {
            var dotenvPath = GetEnvFilePath();

            DotEnv.Load(options: new DotEnvOptions(
                envFilePaths: new[] { dotenvPath }
            ));
            var dotenv = DotEnv.Read(options: new DotEnvOptions(
                envFilePaths: new[] { dotenvPath }
            ));

            Console.Write("dotenv = ", dotenv);

            WORKFLOW_ID = dotenv["WORKFLOW_ID"];
            TRACE_ID = dotenv["TRACE_ID"];
            MY_GROUP = dotenv["MY_GROUP"];
            OTHER_GROUP = dotenv["OTHER_GROUP"];
            ACCOUNT_API_URL = dotenv["ACCOUNT_API_URL"];
            TRACE_API_URL = dotenv["TRACE_API_URL"];
            MEDIA_API_URL = dotenv["MEDIA_API_URL"];

            // Bot 1
            PEM_PRIVATEKEY = dotenv["PEM_PRIVATEKEY"].Replace("\\n", "\n");

            // // Bot 2
            PEM_PRIVATEKEY_2 = dotenv["PEM_PRIVATEKEY_2"].Replace("\\n", "\n");
        }
    }
}