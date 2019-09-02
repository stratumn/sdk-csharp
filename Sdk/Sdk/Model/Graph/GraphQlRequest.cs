using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stratumn.Sdk.Model.Graph
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Http;
    using global::GraphQL.Client;
    using global::GraphQL.Common.Request;
    using global::GraphQL.Common.Response;

    /// <summary>
    ///*
    /// A wrapper class for executing the GraphQL request and returning the response.
    /// 
    /// </summary>
    public class GraphQlRequest
    {
        /// <summary>
        /// Executes the query and returns a responseEntity of type passed </summary>
        /// <param name="url"> </param>
        /// <param name="auth"> </param>
        /// <param name="query"> </param>
        /// <param name="Variables"> </param>
        /// <param name="tClass">
        /// @return </param>
        public static async Task<GraphQLResponse> RequestAsync(string url, string auth, string query, IDictionary<string, object> variables, Type tClass)
        {

           
                GraphQLClient graphClient = new GraphQLClient(url);
                graphClient.DefaultRequestHeaders.Add("Authorization", auth);
                var request = new GraphQLRequest
                {
                    Query = query,
                    Variables = variables
                };

                return await graphClient.PostAsync(request);


        }

    }
}
