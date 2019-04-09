using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace BroadcastComposerWeb.App_Start
{
    public class PreflightRequestsHandler : DelegatingHandler
    {
        private readonly string _AllowedOrigins;
        private readonly string _AllowedHeaders;
        private readonly string _AllowedMethods;
        public PreflightRequestsHandler(string allowedOrigins, string allowedHeaders, string allowedMethods) : base()
        {
            _AllowedOrigins = allowedOrigins;
            _AllowedHeaders = allowedHeaders;
            _AllowedMethods = allowedMethods;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Headers.Contains("Origin") && request.Method.Method.Equals("OPTIONS"))
            {
                var response = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
                // Define and add values to variables: origins, headers, methods (can be global)               
                response.Headers.Add("Access-Control-Allow-Origin", _AllowedOrigins);
                response.Headers.Add("Access-Control-Allow-Headers", _AllowedHeaders);
                response.Headers.Add("Access-Control-Allow-Methods", _AllowedMethods);
                var tsc = new TaskCompletionSource<HttpResponseMessage>();
                tsc.SetResult(response);
                return tsc.Task;
            }
            return base.SendAsync(request, cancellationToken);
        }

    }
}