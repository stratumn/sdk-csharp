using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StratumSdk.Model.Clients
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
            if (endpoints == null)
            {
                throw new System.ArgumentException("endpoints cannot be null");
            }
            if (secret == null)
            {
                throw new System.ArgumentException("secret cannot be null");
            }
            this.endpoints = endpoints;
            this.Secret = secret;
        }


    }

}
