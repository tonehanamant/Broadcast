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

    public class ApiTokenManager : BroadcastBaseClass, IApiTokenManager
    {
        private const string _UmServiceEndpoint = "api/v1/Security/svcToken";
        private static readonly ConcurrentDictionary<string, ApiTokenInfo> _apiTokenMap = new ConcurrentDictionary<string, ApiTokenInfo>(StringComparer.OrdinalIgnoreCase);

        public ApiTokenManager(
                IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper)
            : base(featureToggleHelper, configurationSettingsHelper)
        {
        }

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
                // Token Expiration date is expressed in UTC
                if (TokenExpiryCheckHelper.HasTokenExpired(apiTokenInfo.ExpirationDate, DateTime.UtcNow))
                {
                    apiTokenInfo = await _UpdateApiTokenAsync(umApiBaseUrl, appName, applicationId);
                }
                else
                {
                    _LogInfo($"Reusing the Token for app '{appName}' with expiration '{_GetExpirationDateString(apiTokenInfo.ExpirationDate)}'.");
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
            
            _LogInfo($"Retrieved a new Token for app '{appName}' with expiration '{_GetExpirationDateString(apiTokenInfo.ExpirationDate)}'.");

            return apiTokenInfo;
        }

        private static string _GetExpirationDateString(DateTime? candidate)
        {
            if (candidate.HasValue)
            {
                return candidate.Value.ToString("MM/dd/yyyy HH:mm:ss");
            }
            return "NotSet";
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
                    throw new InvalidOperationException(response.ReasonPhrase);
                }

                return response.Content.ReadAsAsync<TokenResponse>().GetAwaiter().GetResult();
            }
        }
    }
}