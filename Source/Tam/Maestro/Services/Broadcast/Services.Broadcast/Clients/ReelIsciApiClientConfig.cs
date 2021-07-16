using Tam.Maestro.Common;

namespace Services.Broadcast.Clients
{
    public class ReelIsciApiClientConfig
    {
        private readonly IConfigurationSettingsHelper _ConfigurationSettingsHelper;

        public string ApiUrl => _GetApiUrl();

        public string ApiKey => _GetApiKey();

        public ReelIsciApiClientConfig(IConfigurationSettingsHelper configurationSettingsHelper)
        {
            _ConfigurationSettingsHelper = configurationSettingsHelper;
        }

        private string _GetApiUrl()
        {
            var apiUrl = _ConfigurationSettingsHelper.GetConfigValue<string>(ReelIsciApiClientConfigKeys.ApiUrlBase);
            return apiUrl;
        }

        private string _GetApiKey()
        {
            var encryptedDevApiKey = _ConfigurationSettingsHelper.GetConfigValue<string>(ReelIsciApiClientConfigKeys.EncryptedApiKey);
            var apiKey = EncryptionHelper.DecryptString(encryptedDevApiKey, EncryptionHelper.EncryptionKey); ;
            return apiKey;
        }
    }
}