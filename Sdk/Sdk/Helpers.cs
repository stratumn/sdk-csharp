using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Org.BouncyCastle.Security;
using Stratumn.Sdk.Model.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Stratumn.Sdk.Model.Misc;

namespace Stratumn.Sdk
{
    static class Helpers
    {
        public static Int64 GetTime()
        {
            Int64 retval = 0;
            var st = new DateTime(1970, 1, 1);
            TimeSpan t = (DateTime.Now.ToUniversalTime() - st);
            retval = (Int64)(t.TotalMilliseconds + 0.5);
            return retval;
        }
        public static Endpoints MakeEndpoints(Endpoints endpoints)
        {

            const string ACCOUNT_RELEASE_URL = "https://account-api.staging.stratumn.com";
            const string TRACE_RELEASE_URL = "https://trace-api.staging.stratumn.com";
            const string MEDIA_RELEASE_URL = "https://media-api.staging.stratumn.com";

            if (endpoints == null)
            {
                return new Endpoints(ACCOUNT_RELEASE_URL, TRACE_RELEASE_URL, MEDIA_RELEASE_URL);
            }


            if (endpoints.Account == null || endpoints.Trace == null || endpoints.Media == null)
            {
                throw new InvalidParameterException("The provided endpoints argument is not valid.");
            }
            return endpoints;
        }
        /// <summary>
        /// 
        /// Extract all filewrappers from some data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Dictionary<string, Property<FileWrapper>> ExtractFileWrappers<T>(T data)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Extract all file records from some data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Dictionary<string, Property<FileRecord>> ExtractFileRecords<T>(T data)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Replaces on object with another based on the path provided both objects extend Identifiable interface
        /// fieds/arrays are defined using that interface
        /// </summary>
        /// <param name="fileRecordList"></param>
        public static void AssignObjects<V>(List<Property<V>> propertyList) where V : Identifiable
        {
            throw new NotImplementedException();
        }
    }
}
