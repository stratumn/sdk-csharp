using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Helpers;
using System.Web.Script.Serialization;
using BCrypt.Net;
using Utils;
using Org.BouncyCastle.Crypto;
using Stratumn.Chainscript;
using Stratumn.Sdk.Model.Client;
using GraphQL.Common.Response;
using Lucene.Net.Support;
using Stratumn.Sdk.Model.File;
using System.Net.Http.Headers;
using Stratumn.Chainscript.utils;
using System.Threading;
using GraphQL.Client;
using GraphQL.Common.Request;

namespace Stratumn.Sdk
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

        private WebProxy proxy;

        private GraphQLOptions defaultGraphQLOptions = new GraphQLOptions(1);

        public Client(ClientOptions opts)
        {
            this.endpoints = Helpers.MakeEndpoints(opts.Endpoints);

            this.secret = opts.Secret;
            this.proxy = opts.Proxy;
        }


        public string GetBearerToken(string pem)
        {
            // create message to be signed in json
            long nowInSeconds = Helpers.GetTime() / 1000;
            var timeJson = new
            {
                iat = nowInSeconds,
                exp = nowInSeconds + 10 * 60
            };
            byte[] message = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(timeJson));


            string messagebase64 = Convert.ToBase64String(message);

            var signature = Signature.Sign(CryptoUtils.DecodeEd25519PrivateKey(pem).GetEncoded(), message);

            // create the payload
            var keysJson = new
            {
                signature = Convert.ToBase64String(signature.ByteSignature()),
                message = messagebase64,
                public_key = Convert.ToBase64String(signature.PublicKey())
            };

            // convert the payload to base64
            string cleanedSignedToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(keysJson)));

            return cleanedSignedToken;


        }


        /**
    * To set a new token
    * 
    * @param token the new token
    */
        private void SetToken(String token)
        {
            this.token = token;
        }

        /**
         * To clear the existing token
         */
        private void ClearToken()
        {
            this.token = null;
        }



        //
        // Authenticates using a valid secret. Supported secret types are: -
        // CredentialSecret -> via email+password - PrivateKeySecret -> via signed
        // message
        ///
        public async Task<string> Login()
        {

            // otherwise do the job...
            if (Secret.IsCredentialSecret(this.secret))
            {
                // the CredentialSecret case
                string email = ((CredentialSecret)this.secret).Email;
                string password = ((CredentialSecret)this.secret).Password;
                return await this.LoginWithCredentials(email, password);

            }
            else if (Secret.IsPrivateKeySecret(this.secret))
            {
                // the PrivateKeySecret case
                string privateKey = ((PrivateKeySecret)this.secret).PrivateKey;
                return await this.LoginWithSigningPrivateKey(privateKey);

            }
            else if (Secret.IsProtectedKeySecret(this.secret))
            {
                // the ProtectedKeySecret cas
                throw new NotImplementedException("Authentication via password protected key is not handled");
            }
            else
            {
                // Unknown case
                throw new ApplicationException("The provided secret does not have the right format");
            }

        }


        /// <summary>
        /// Utility method to fetch a ressource on a target service via REST.
        /// </summary>
        /// <param name="request"> its instance from HttpHelpers </param>
        /// <exception cref="HttpError">
        /// @returns the responseContent </exception>

        private async Task<HttpResponseMessage> FetchAsync<T>(HttpRequestMessage request, HttpClient client, int retry)
        {

            HttpResponseMessage response = client.SendAsync(request).Result;


            bool _ok = (HttpStatusCode.OK == response.StatusCode);
            if (!_ok)
            {

                // if 401 and retry > 0 then we can retry
                if (response.StatusCode.ToString() == "401" && retry > 0)
                {
                    // unauthenticated request might be because token expired
                    // clear token and retry
                    this.ClearToken();

                    return await FetchAsync<T>(request, client, --retry);
                }

                // otherwise that's a proper error
                // extract the text body of the response
                // and try to convert it to JSON
                string errTxt = response.ReasonPhrase;

                // throw that new error
                throw new HttpError((int)response.StatusCode, errTxt);

            }

            return response;

        }



        /// <summary>
        /// Executes a POST query on a target service.
        /// </summary>
        /// <param name="service"> the service to target (account|trace|media) </param>
        /// <param name="route">   the route on the target service </param>
        /// <param name="body">    the POST body object </param>
        /// <param name="opts">    additional fetch options </param>
        /// <exception cref="TraceSdkException"> 
        /// @returns the response body object </exception>
        public async Task<HttpResponseMessage> PostAsync<T>(Service service, string route, string body, FetchOptions opts)
        {

            //create default fetch options.
            if (opts == null)
            {
                opts = new FetchOptions();
            }

            // References: https://www.baeldung.com/java-http-request
            // https://juffalow.com/java/how-to-send-http-get-post-request-in-java
            string path = this.endpoints.GetEndpoint(service) + '/' + route;

            Uri url = new Uri(path);

            //  string strPayload = JsonConvert.SerializeObject(payload);
            HttpContent httpContent = new StringContent(body, Encoding.UTF8, "application/json");


            using (var client = new HttpClient(CreateHttpMessageHandler()))
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("Authorization", await this.GetAuthorizationHeader(opts));

                HttpRequestMessage requestClient = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = httpContent
                };


                int _retry = opts.Retry;

                // delegate to fetch wrapper
                return await FetchAsync<T>(requestClient, client, _retry);
            }

        }



        /// <summary>
        /// Executes a GET query on a target service.
        /// </summary>
        /// <param name="service"> the service to target (account|trace|media) </param>
        /// <param name="route">   the route on the target service </param>
        /// <param name="params">  the query parameters </param>
        /// <param name="opts">    additional fetch options </param>
        /// <exception cref="TraceSdkException"> 
        /// @returns the response body object </exception>
        public async Task<HttpResponseMessage> GetAsync<T>(Service service, string route, IDictionary<string, string> parameters, FetchOptions opts)
        {


            //create default fetch options.
            if (opts == null)
            {
                opts = new FetchOptions();
            }

            string path = this.endpoints.GetEndpoint(service) + '/' + route;
            var builder = new UriBuilder(path);
            if (parameters != null)
            {
                builder.Query = string.Join("&", parameters.Select(x => $"{x.Key}={x.Value}"));
            }


            using (var client = new HttpClient(CreateHttpMessageHandler()))
            {
                client.DefaultRequestHeaders.Accept.Clear();

                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("Authorization", await this.GetAuthorizationHeader(opts));

                HttpRequestMessage requestClient = new HttpRequestMessage(HttpMethod.Get, builder.Uri);

                int _retry = opts.Retry;

                // delegate to fetch wrapper
                var clientResponse = await FetchAsync<T>(requestClient, client, _retry);

                if (clientResponse.StatusCode != HttpStatusCode.OK)
                {
                    string res = clientResponse.StatusCode + ":";
                    string error = clientResponse.Content.ReadAsStringAsync().Result;
                    throw new ApplicationException(res + error);
                }
                return clientResponse;
            }

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
            var builder = new UriBuilder(getSaltUrl)
            {
                Query = string.Join("&", parameters.Select(x => $"{x.Key}={x.Value}"))
            };


            HttpClient client = new HttpClient(CreateHttpMessageHandler());
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
        public class GroupResponse
        {
            public string GroupId
            {
                get;
                set;
            }

            public string AccountId
            {
                get;
                set;
            }

        }

        /// <summary>
        /// Retrieves an authentication token based on the following waterfall: - if
        /// opts.authToken is set, use it to compute the auth header - if opts.skipAuth
        /// is true, return empty auth header - otherwise login and use the retrieved
        /// token to compute the auth header
        /// </summary>
        /// <param name="opts">           optional options </param>
        /// <param name="opts.authToken"> optional token to be used </param>
        /// <param name="opts.skipAuth">  optional flag to bypass authentication </param>
        /// <exception cref="TraceSdkException">   </exception>

        public async Task<string> GetAuthorizationHeader(FetchOptions opts)
        {

            if (opts != null)
            {
                if (opts.AuthToken != null)
                {
                    return this.MakeAuthorizationHeader(opts.AuthToken);
                }
                if (opts.SkipAuth != null)
                {
                    return this.MakeAuthorizationHeader(null);
                }
            }

            await this.Login();

            return this.MakeAuthorizationHeader(this.token);
        }




        /// <summary>
        /// Authenticate using a signed message via the GET /login route.
        /// @param key the signing private key in clear text used to log in
        /// </summary>
        private async Task<string> LoginWithSigningPrivateKey(string pem)
        {

            var signinToken = this.GetBearerToken(pem);


            HttpResponseMessage clientResponse = await this.GetAsync<string>(Service.ACCOUNT, "login", null, new FetchOptions(signinToken, false, 1));

            string jsonResponse = await clientResponse.Content.ReadAsStringAsync();
            var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(jsonResponse);
            this.SetToken(tokenResponse.Token);
            return tokenResponse.Token;

        }

        /// <summary>
        ///  Uploads an array of files to media api 
        /// </summary>
        /// <param name="fileList"></param>
        /// <returns></returns>
        public async Task<MediaRecord[]> UploadFiles(List<FileWrapper> fileWrapperList)
        {
            if (fileWrapperList.Count == 0) return new MediaRecord[0];
            String path = this.endpoints.Media + "/files";
            MediaRecord[] mediaRecords = null;
            mediaRecords = await UploadFiles<MediaRecord[]>(path, fileWrapperList);

            return mediaRecords;

        }


        /// <summary>
        /// Uploads multiple files to the server path 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="fileWrapperList"></param>
        /// <returns></returns>
        private async Task<T> UploadFiles<T>(String path, List<FileWrapper> fileWrapperList)
        {
            Uri url = new Uri(path);
            HttpResponseMessage response;
            using (var client = new HttpClient(CreateHttpMessageHandler()))
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("Authorization", await this.GetAuthorizationHeader(null));
                MultipartFormDataContent filesContent = new MultipartFormDataContent("--------------");
                foreach (FileWrapper file in fileWrapperList)
                {
                    Model.File.FileInfo fileInfo = file.Info();

                    var fileContent = new StreamContent(file.EncryptedData());
                    fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                    {
                        Name = "\"" + fileInfo.Name + "\"",
                        FileName = "\"" + fileInfo.Name + "\""
                    };


                    filesContent.Add(fileContent);
                }
                response = await client.PostAsync(url, filesContent);
            }
            return JsonHelper.ObjectToObject<T>(response.Content.ReadAsStringAsync().Result);
        }



        /// <summary>
        // Authenticates using a user's credentials via the POST /login route.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        private async Task<string> LoginWithCredentials(string email, string password)
        {
            IDictionary<string, string> parameters = new Dictionary<string, string>() { { "email", email } };
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


            using (var client = new HttpClient(CreateHttpMessageHandler()))
            {
                HttpResponseMessage responseMessage = client.PostAsync(loginAccountUri, c).Result;

                if (responseMessage.IsSuccessStatusCode)
                {
                    string jsonResponse = responseMessage.Content.ReadAsStringAsync().Result;
                    var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(jsonResponse);

                    // finally set the new token
                    this.SetToken(tokenResponse.Token);
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

        public async Task<MemoryStream> DownloadFile(FileRecord fileRecord)
        {

            HttpResponseMessage clientResponse = await this.GetAsync<String>(Service.MEDIA, "files/" + fileRecord.Digest + "/info", null, null);

            string jsonResponse = await clientResponse.Content.ReadAsStringAsync();
            var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(jsonResponse);
            this.SetToken(tokenResponse.Token);
            return null;// return tokenResponse.Token; 
        }

        /// <summary>
        /// Executes a GraphQL query / mutation on the Trace service.
        /// </summary>
        /// <param name="query">     the graphql query / mutation </param>
        /// <param name="variables"> the graphql variables </param>
        /// <param name="opts">      the graphql options </param>
        /// <exception cref="TraceSdkException"> 
        ///  </exception>
        public async Task<GraphQLResponse> GraphqlAsync(String query, IDictionary<string, object> variables, GraphQLOptions opts, Type tclass)
        {

            String gqlUrl = this.endpoints.Trace + "/graphql";

            // delegate the graphql request execution 
            GraphQLResponse response = await GraphqlExecute(gqlUrl, await this.GetAuthorizationHeader(null), query, variables, tclass);
            if (opts == null)
            {
                opts = defaultGraphQLOptions;
            }

            if (response.Errors == null)
            {
                // if the response is empty, throw.
                if (response.Data == null)
                {
                    throw new TraceSdkException("The graphql response is empty.");
                }
            }
            else
            {
                var statusCode = response.Errors.LastOrDefault()?.AdditonalEntries?.Values.ToArray()[0];

                int retry = opts.Retry;
                // handle errors explicitly 
                // extract the status from the error response 
                // if 401 and retry > 0 then we can retry
                if (statusCode != null && statusCode.ToString() == "401" && retry > 0)
                {
                    // unauthenticated request might be because token expired
                    // clear token and retry
                    this.ClearToken();
                    opts.Retry = opts.Retry - 1;
                    return await this.GraphqlAsync(query, variables, opts, tclass);
                }
                // otherwise rethrow
                throw new TraceSdkException(response.Errors[0].Message);
            }
            return response;

        }

        private HttpMessageHandler CreateHttpMessageHandler()
        {
            LoggingHandler httpHandler;
            if (proxy != null)
                httpHandler = new LoggingHandler(new HttpClientHandler
                {
                    Proxy = this.proxy
                });
            else
                httpHandler = new LoggingHandler(new HttpClientHandler());
            return httpHandler;
        }


        /// <summary>
        /// Executes the query and returns a responseEntity of type passed </summary>
        /// <param name="url"> </param>
        /// <param name="auth"> </param>
        /// <param name="query"> </param>
        /// <param name="Variables"> </param>
        /// <param name="tClass">
        /// @return </param>
        public async Task<GraphQLResponse> GraphqlExecute(string url, string auth, string query, IDictionary<string, object> variables, Type tClass)
        {

            GraphQLClientOptions clientOptions = new GraphQLClientOptions();
            clientOptions.HttpMessageHandler = CreateHttpMessageHandler();
            clientOptions.JsonSerializerSettings = new JsonSerializerSettings()
            {

                ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver()
            };


            GraphQLClient graphClient = new GraphQLClient(url, clientOptions);
            graphClient.DefaultRequestHeaders.Add("Authorization", auth);
            var request = new GraphQLRequestCamel(query, variables);

            return await graphClient.PostAsync(request);


        }
    }


    public class GraphQLRequestCamel : GraphQLRequest
    {


        public GraphQLRequestCamel(string query, dynamic Variables)
        {

            base.Query = query;
            base.Variables = Variables;
        }
        [JsonProperty(PropertyName = "query")]
        public string CamelCaseQuery
        {
            get
            {
                return base.Query;


            }

        }


        [JsonProperty(PropertyName = "variables")]
        public dynamic CamelCaseVariables
        {
            get
            {

                return base.Variables;
            }

        }
    }

    class LoggingHandler : DelegatingHandler
    {
        public LoggingHandler(HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Debug.WriteLine("===========================request begin================================================");
            Debug.WriteLine("URI         : " + request.RequestUri);
            Debug.WriteLine("Method      : " + request.Method);
            Debug.WriteLine("Headers     : " + request.Headers);
            if (request.Content != null)
                Debug.WriteLine("Request body: " + await request.Content.ReadAsStringAsync());
            Debug.WriteLine("==========================request end================================================");


            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            Debug.WriteLine("============================response begin==========================================");
            Debug.WriteLine("Status code  : " + response.StatusCode);
            Debug.WriteLine("Status text  : " + response.ReasonPhrase);
            Debug.WriteLine("Headers      : " + response.Headers);
            if (response.Content != null)
                Debug.WriteLine("Response body: " + await response.Content.ReadAsStringAsync());
            Debug.WriteLine("=======================response end=================================================");

            return response;
        }
    }

}

