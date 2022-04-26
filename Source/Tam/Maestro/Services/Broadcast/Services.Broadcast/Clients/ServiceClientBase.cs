using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Services.Broadcast.Clients
{
    public interface IServiceClientBase
    {
        HttpClient GetServiceHttpClient(string svcBaseUrl, string applicationId, string bearerToken = "");
    }

    public class ServiceClientBase: IServiceClientBase
    {
        /// <summary>
        /// Returns a HttpClient for use in calling other services.  Will set all required headers.
        /// </summary>
        /// <param name="svcBaseUrl">svcBaseUrl</param>
        /// <param name="applicationId">applicationId</param>
        /// <param name="bearerToken">bearerToken</param>
        /// <returns>HttpClient</returns>
        public HttpClient GetServiceHttpClient(string svcBaseUrl, string applicationId, string bearerToken = "")
        {
            if (string.IsNullOrEmpty(svcBaseUrl))
                throw new ArgumentNullException("Service url can't be null or empty.");

            if (string.IsNullOrEmpty(applicationId))
                throw new ArgumentNullException("Application Id is missing in the request.");

            if (!svcBaseUrl.EndsWith("/"))
                svcBaseUrl += "/";

            return GetSecureHttpClient(svcBaseUrl, applicationId, bearerToken);
        }

        #region Private Methods

        private HttpClient GetSecureHttpClient(string svcBaseUrl, string applicationId, string bearerToken = "")
        {
            var client = new HttpClient();

            client.BaseAddress = new Uri(svcBaseUrl);

            AddCadentHeadersToClient(ref client, applicationId, bearerToken);

            return client;
        }


        private void AddCadentHeadersToClient(ref HttpClient client, string applicationId, string bearerToken = "")
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
