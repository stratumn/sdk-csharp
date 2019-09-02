using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stratumn.Sdk.Model.Client
{
    /// <summary>
    /// The fetch options
    /// </summary>
    public class FetchOptions
    {
        /// <summary>
        /// The authentication to use (will not use the token provided via automatic
        /// login) defaults to undefined
        /// </summary>
        internal string authToken;

        /// <summary>
        /// Flag to bypass the automatic login mechanism defaults to false
        /// </summary>
        internal bool? skipAuth;

        /// <summary>
        /// The retry count defaults to 1
        /// </summary>
        internal int retry = 1;

        public FetchOptions()
        {

            this.skipAuth = false;
            this.retry = 1;
        }


        public FetchOptions(string authToken, bool? skipAuth, int retry)
        {

            this.authToken = authToken;
            this.skipAuth = skipAuth;
            if (retry != null)
            {
                this.retry = retry;
            }
        }

        public string AuthToken
        {
            get
            {
                return this.authToken;
            }
            set
            {
                this.authToken = value;
            }
        }


        public bool? SkipAuth
        {
            get
            {
                return this.skipAuth;
            }
            set
            {
                this.skipAuth = value;
            }
        }


        public int Retry
        {
            get
            {
                return this.retry;
            }
            set
            {
                this.retry = value;
            }
        }


    }
}
