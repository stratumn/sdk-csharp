using System;
using System.Net;

namespace Stratumn.Sdk.Model.Client
{

    /// <summary>
    /// Options Class used to instantiate the Client.
    /// </summary>
    public class ClientOptions
    {
        /// <summary>
        /// To configure the endpoints. Can be a short tag like 'release' or 'staging'.
        /// Can also be a struct to configure each service endpoint, eg: { trace: 'https://...' .. }.
        /// Defaults to release endpoints.
        /// </summary>
        public Endpoints Endpoints { get; set; }


        public Secret Secret { get; set; }

        
        public ClientOptions(Endpoints endpoints, Secret secret)
        {
            this.Endpoints = endpoints;
            this.Secret = secret;
        }


        public void setProxy(string host, int port)
        {
            if (host == null)
                throw new  ArgumentException("host cannot be null in proxy");
            if (port == 0)
                throw new ArgumentException("port cannot be 0 in proxy");

            this.Proxy = new WebProxy(host, port);
        }

        public WebProxy Proxy
        {
            get; set;
        }

    }

}
