using Org.BouncyCastle.Security;
using StratumSdk.Model.Clients;
using System;

namespace stratumn.sdk
{
    static class Helper
    {
        public static Int64 GetTime()
        {
            Int64 retval = 0;
            var st = new DateTime(1970, 1, 1);
            TimeSpan t = (DateTime.Now.ToUniversalTime() - st);
            retval = (Int64)(t.TotalMilliseconds + 0.5);
            return retval;
        }
        public static Endpoints makeEndpoints(Endpoints endpoints)
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

    }
}
