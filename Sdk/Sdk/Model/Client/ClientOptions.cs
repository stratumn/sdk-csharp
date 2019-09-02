namespace Stratumn.Sdk.Model.Client
{

    /// <summary>
    /// Options Class used to instantiate the Client.
    /// </summary>
    public class ClientOptions
    {

        public Endpoints Endpoints { get; set; }


        public Secret Secret { get; set; }
        /// <summary>
        /// To configure the endpoints. Can be a short tag like 'release' or 'staging'.
        /// Can also be a struct to configure each service endpoint, eg: { trace: 'https://...' .. }.
        /// Defaults to release endpoints.
        /// </summary>
        private Endpoints endpoints;



        public ClientOptions(Endpoints endpoints, Secret secret)
        {
            this.endpoints = endpoints;
            this.Secret = secret;
        }


    }

}
