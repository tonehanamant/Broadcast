using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Locking;
using Services.Broadcast.Extensions;
using Services.Broadcast.Helpers;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Services.Broadcast.Clients
{
    public interface IInventoryManagementApiClient
    {
       
    }
    public class InventoryManagementApiClient : CadentSecuredClientBase, IInventoryManagementApiClient
    {
        public InventoryManagementApiClient(IApiTokenManager apiTokenManager,
                 IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper)
             : base(apiTokenManager, featureToggleHelper, configurationSettingsHelper)
        {
        }       
        private async Task<HttpClient> _GetSecureHttpClientAsync()
        {
            var apiBaseUrl = _GetApiUrl();
            var applicationId = _GetApplicationId();
            var appName = _GetAppName();
            var client = await _GetSecureHttpClientAsync(apiBaseUrl, applicationId, appName);            
            return client;
        }

        private string _GetApiUrl()
        {
            var apiUrl = _ConfigurationSettingsHelper.GetConfigValue<string>(InventoryManagementApiConfigKeys.ApiBaseUrl);

            return apiUrl;
        }
        private string _GetApplicationId()
        {
            var applicationId = _ConfigurationSettingsHelper.GetConfigValue<string>(InventoryManagementApiConfigKeys.ApplicationId);
            return applicationId;
        }
        private string _GetAppName()
        {
            var appName = _ConfigurationSettingsHelper.GetConfigValue<string>(InventoryManagementApiConfigKeys.AppName);
            return appName;
        }
    }
}
