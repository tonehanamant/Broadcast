using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Services.Broadcast.Clients
{
    /// <summary>
    /// A helper class to get an HttpClient with all Cadent headers configured.
    /// </summary>
    public static class CadentServiceClientHelper
    {
        /// <summary>
        /// Returns a HttpClient for use in calling other services.  Will set all required headers.
        /// </summary>
        public static HttpClient GetServiceHttpClient(string svcBaseUrl, string applicationId, string bearerToken = "")
        {
            if (string.IsNullOrEmpty(svcBaseUrl))
            {
                throw new ArgumentNullException(svcBaseUrl, "Service url can't be null or empty.");
            }

            if (string.IsNullOrEmpty(applicationId))
            {
                throw new ArgumentNullException(applicationId, "Application Id is missing in the request.");
            }

            if (!svcBaseUrl.EndsWith("/"))
            {
                svcBaseUrl += "/";
            }

            return GetSecureHttpClient(svcBaseUrl, applicationId, bearerToken);
        }

        #region Private Methods

        private static HttpClient GetSecureHttpClient(string svcBaseUrl, string applicationId, string bearerToken = "")
        {
            var client = new HttpClient();

            client.BaseAddress = new Uri(svcBaseUrl);

            AddCadentHeadersToClient(ref client, applicationId, bearerToken);

            return client;
        }


        private static void AddCadentHeadersToClient(ref HttpClient client, string applicationId, string bearerToken = "")
        {
            client.DefaultRequestHeaders.Add("application_id", applicationId);
            client.DefaultRequestHeaders.Add("entity_id", "CADENT");
            client.DefaultRequestHeaders.Add("transaction_id", Guid.NewGuid().ToString());

            if (!string.IsNullOrEmpty(bearerToken))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        }

        #endregion

    }

}
