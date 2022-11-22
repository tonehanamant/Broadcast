using Newtonsoft.Json;
using Services.Broadcast.Entities;
using Services.Broadcast.Helpers;
using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Clients
{
    public class ApiTokenInfo
    {
        public string Jwt { get; set; }

        public DateTime? ExpirationDate { get; set; }

    }

    public interface IApiTokenManager
    {
        Task<string> GetOrRefreshTokenAsync(string umApiBaseUrl, string appName, string applicationId);
    }

    public class ApiTokenManager :  IApiTokenManager
    {
        private const string _UmServiceEndpoint = "api/v1/Security/svcToken";
        private static readonly ConcurrentDictionary<string, ApiTokenInfo> _apiTokenMap = new ConcurrentDictionary<string, ApiTokenInfo>(StringComparer.OrdinalIgnoreCase);

        public async Task<string> GetOrRefreshTokenAsync(string umApiBaseUrl, string appName, string applicationId)
        {
            if (string.IsNullOrWhiteSpace(umApiBaseUrl))
                throw new ArgumentNullException(nameof(umApiBaseUrl));

            if (string.IsNullOrWhiteSpace(appName))
                throw new ArgumentNullException(nameof(appName));

            if (string.IsNullOrWhiteSpace(applicationId))
                throw new ArgumentNullException(nameof(applicationId));

            if (_apiTokenMap.TryGetValue(appName, out var apiTokenInfo))
            {
                if (TokenExpiryCheckHelper.HasTokenExpired(apiTokenInfo.ExpirationDate, DateTime.Now))
                {
                    apiTokenInfo = await _UpdateApiTokenAsync(umApiBaseUrl, appName, applicationId);
                }

                return apiTokenInfo.Jwt;

            }
            else
            {
                apiTokenInfo = await _UpdateApiTokenAsync(umApiBaseUrl, appName, applicationId);
                return apiTokenInfo.Jwt;
            }
        }

        private async Task<ApiTokenInfo> _UpdateApiTokenAsync(string umUrl, string appName, string applicationId)
        {
            var accessToken = _GetAccessToken(umUrl, appName, applicationId);
            var apiTokenInfo = new ApiTokenInfo
            {
                Jwt = accessToken.AccessToken,
                ExpirationDate = accessToken.Expires
            };

            _apiTokenMap[appName] = apiTokenInfo;

            return apiTokenInfo;
        }

        public TokenResponse _GetAccessToken(string umUrl,string appName, string applicationId)
        {
            using (var client = CadentServiceClientHelper.GetServiceHttpClient(umUrl.EndsWith("/") ? umUrl : $"{umUrl}/", applicationId))
            {
                var req = new SvcTokenRequest
                {
                    SvcName = appName
                };

                var response = client.PostAsync(_UmServiceEndpoint, new StringContent(JsonConvert.SerializeObject(req),
                    Encoding.UTF8, "application/json"))
                    .GetAwaiter().GetResult();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception(response.ReasonPhrase);
                }

                return response.Content.ReadAsAsync<TokenResponse>().GetAwaiter().GetResult();
            }
        }
    }
}