namespace StratumSdk.Model.Clients
{
    public class Endpoints
    {
        public string Trace { get; set; }
        public string Account { get; set; }
        public string Media { get; set; }

        //  private static String TRACE_RELEASE_URL = "https://trace.stratumn.com";
        //  private static String ACCOUNT_RELEASE_URL = "https://account.stratumn.com";
        //  private static String MEDIA_RELEASE_URL = "https://media.stratumn.com";
        //  

        internal readonly string ACCOUNT_RELEASE_URL = "https://account-api.staging.stratumn.com";
        internal readonly string TRACE_RELEASE_URL = "https://trace-api.staging.stratumn.com";
        internal readonly string MEDIA_RELEASE_URL = "https://media-api.staging.stratumn.com";


        public Endpoints()
        {
            this.Account = ACCOUNT_RELEASE_URL;
            this.Trace = TRACE_RELEASE_URL;
            this.Media = MEDIA_RELEASE_URL;
        }

        public Endpoints(string account, string trace, string media)
        {
            this.Account = account;
            this.Trace = trace;
            this.Media = media;
        }

        public string ReleaseEndpoints(Service service)
        {
            switch (service)
            {
                case Service.ACCOUNT:
                    return this.Account;

                case Service.TRACE:
                    return Trace;

                case Service.MEDIA:
                    return Media;

            }
            return null;
        }
    }
}
