using Newtonsoft.Json;
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using StratumSdk.Model.Clients;

namespace stratumn.sdk
{


    public class Client
    {

        //
        // The endpoint urls for all the services
        ///
        private Endpoints endpoints;
        //
        // The secret used to authenticate
        ///
        private Secret secret;
        //
        // The token received from account service after authentication
        ///
        private string token;

        //
        // The mutex used to prevent concurrent login requests
        ///
        // private Mutex mutex;

        public Client(ClientOptions opts)
        {
            this.endpoints = Helper.makeEndpoints(opts.Endpoints);

            this.secret = opts.Secret;
            // this.mutex = new Mutex();
        }


        public string GetBearerToken(string pem)
        {

            string publicKeyPem = Regex.Unescape(Settings.Default.publicKey);

            Ed25519PrivateKeyParameters privateKey = CryptoUtils.DecodeEd25519PrivateKey(pem);

            // create message to be signed in json
            long nowInSeconds = Helper.GetTime() / 1000;
            var timeJson = new
            {
                iat = nowInSeconds,
                exp = nowInSeconds + 10 * 60
            };
            byte[] message = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(timeJson));

            // sign the json string
            var signature = CryptoUtils.Sign(privateKey, JsonConvert.SerializeObject(timeJson));

            // convert the message, the signature and the publickKey to base
            string messagebase64 = Convert.ToBase64String(message);
            string signaturebase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(signature));
            string publickKeyBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(publicKeyPem));

            // create the payload
            var keysJson = new
            {
                signature = signaturebase64,
                message = messagebase64,
                public_key = publickKeyBase64
            };

            // convert the payload to base64
            string cleanedSignedToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(keysJson)));

            return cleanedSignedToken;


        }


        //
        // Authenticates using a valid secret. Supported secret types are: -
        // CredentialSecret -> via email+password - PrivateKeySecret -> via signed
        // message
        ///
        public async Task<string> Login()
        {

            // acquire the mutex
            // final release = await this.mutex.acquire();

            // if another concurrent execution has already
            // done the job, then release and return, nothing to do.

            // if (this.token) { release(); return; }

            // otherwise do the job...
            if (SdkSecret.IsCredentialSecret(this.secret))
            {
                // the CredentialSecret case
                string email = this.secret.Email;
                string password = this.secret.Password;
                return await this.LoginWithCredentials(email, password);

            }
            else if (SdkSecret.IsPrivateKeySecret(this.secret))
            {
                // the PrivateKeySecret case
                string privateKey = this.secret.PrivateKey;
                return await this.LoginWithSigningPrivateKey(privateKey);

            }
            else if (SdkSecret.IsProtectedKeySecret(this.secret))
            {
                // the ProtectedKeySecret cas
                throw new NotImplementedException("Authentication via password protected key is not handled");
            }
            else
            {
                // Unknown case
                throw new ApplicationException("The provided secret does not have the right format");
            }

            //Todo:
            //in case no error were thrown, release here
            // release();
        }

        //
        // Compute the bearer Authorization header of format "Bearer my_token". If the
        // token is undefined, the return header is an empty string "".
        // @param token optional token to be used
        ///
        private string MakeAuthorizationHeader(string token)
        {
            return (token != null) ? "Bearer " + token : "";
        }

        /// <summary>
        /// Call the salt endpoint 
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private async Task<string> GetSalt(IDictionary<string, string> parameters)
        {

            string getSaltUrl = endpoints.Account + "/salt";
            var builder = new UriBuilder(getSaltUrl);
            builder.Query = string.Join("&", parameters.Select(x => $"{x.Key}={x.Value}"));

            HttpClient client = new HttpClient();
            var result = client.GetAsync(builder.Uri).Result;

            try
            {
                using (StreamReader sr = new StreamReader(result.Content.ReadAsStreamAsync().Result))
                {
                    var jsonResponse = sr.ReadToEnd();
                    var saltResponse = JsonConvert.DeserializeObject<SaltResponse>(jsonResponse);

                    return saltResponse.Salt;
                }
            }
            catch (WebException e)
            {
                HttpWebResponse response = null;

                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    response = (HttpWebResponse)e.Response;
                    Console.Write("Errorcode: {0}", (int)response.StatusCode);
                }
                else
                {
                    Console.Write("Error: {0}", e.Status);
                }
            }

            return await Task.FromResult<string>(null);
        }

        public class SaltResponse
        {
            public string Salt
            {
                get;
                set;
            }

        }

        public class TokenResponse
        {
            public string Token
            {
                get;
                set;
            }

        }

        /// <summary>
        /// To set a new token
        ///@param token the new token
        /// </summary>
        private void setToken(string token)
        {
            this.token = token;
        }

        /// <summary>
        /// To clear the existing token
        /// </summary>
        private void ClearToken()
        {
            this.token = null;
        }

        /// <summary>
        /// Authenticate using a signed message via the GET /login route.
        /// @param key the signing private key in clear text used to log in
        /// </summary>
        private async Task<string> LoginWithSigningPrivateKey(string pem)
        {

            var signinToken = this.GetBearerToken(pem);

            string loginAccountUrl = endpoints.Account + "/login";


            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("Authorization", this.MakeAuthorizationHeader(signinToken));
                HttpResponseMessage clientResponse = await client.GetAsync(loginAccountUrl);


                if (clientResponse.StatusCode == HttpStatusCode.OK)
                {
                    string jsonResponse = await clientResponse.Content.ReadAsStringAsync();
                    var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(jsonResponse);
                    this.setToken(tokenResponse.Token);
                    return tokenResponse.Token;

                }
                else
                {
                    string res = clientResponse.StatusCode + ":";
                    string error = clientResponse.Content.ReadAsStringAsync().Result;
                    throw new ApplicationException(res + error);
                }

            }

        }


        /// <summary>
        // Authenticates using a user's credentials via the POST /login route.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        private async Task<string> LoginWithCredentials(string email, string password)
        {
            IDictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("email", email);
            var salt = await this.GetSalt(parameters);
            // hash the password with the salt
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(password, salt);



            Uri loginAccountUri = new Uri(endpoints.Account + "/login");
            var payload = new Dictionary<string, string>
            {
              {"email", email},
              {"passwordHash", passwordHash}
            };

            string strPayload = JsonConvert.SerializeObject(payload);
            HttpContent c = new StringContent(strPayload, Encoding.UTF8, "application/json");


            using (var client = new HttpClient())
            {
                HttpResponseMessage responseMessage = client.PostAsync(loginAccountUri, c).Result;

                if (responseMessage.IsSuccessStatusCode)
                {
                    string jsonResponse = responseMessage.Content.ReadAsStringAsync().Result;
                    var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(jsonResponse);

                    // finally set the new token
                    this.setToken(tokenResponse.Token);
                    return tokenResponse.Token;
                }
                else
                {
                    string res = responseMessage.StatusCode + ":";
                    string error = responseMessage.Content.ReadAsStringAsync().Result;
                    throw new ApplicationException(res + error);
                }
            }

        }

    }
}

