using Services.Broadcast.Entities.DTO.Program;
using Services.Broadcast.Helpers;
using System;
using System.Collections.Generic;
using System.Net.Http;
using Tam.Maestro.Common.Clients;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace Services.Broadcast.Clients
{
    public interface IProgramsSearchApiClient
    {
        List<SearchProgramDativaResponseDto> GetPrograms(SearchRequestProgramDto searchRequest);
    }

    public class ProgramsSearchApiClient : TokenizedClientBase, IProgramsSearchApiClient
    {
        private readonly string _ProgramsSearchUrl;
        private readonly IConfigurationSettingsHelper _ConfigurationSettingsHelper;
        private readonly IFeatureToggleHelper _FeatureToggleHelper;
        private Lazy<bool> _IsPipelineVariablesEnabled;
        private Lazy<string> _ProgramsSearchClientId;
        private Lazy<string> _ProgramsSearchEncryptedSecret;

        public ProgramsSearchApiClient(IAwsCognitoClient tokenClient, IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper) : base(tokenClient)
        {
            
            _ProgramsSearchUrl = BroadcastServiceSystemParameter.ProgramsSearchUrl;
            _ConfigurationSettingsHelper = configurationSettingsHelper;
            _FeatureToggleHelper = featureToggleHelper;
            _IsPipelineVariablesEnabled = new Lazy<bool>(() => _FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_PIPELINE_VARIABLES));
            _ProgramsSearchClientId = new Lazy<string>(_GetProgramsSearchClientId);
            _ProgramsSearchEncryptedSecret = new Lazy<string>(GetProgramsSearchEncryptedSecret);
           
        }
        
        public List<SearchProgramDativaResponseDto> GetPrograms(SearchRequestProgramDto searchRequest)
        {
            _ConfigureTokenizedClient(BroadcastServiceSystemParameter.ProgramsSearchTokenUrl,
               _ProgramsSearchClientId.Value,
               _ProgramsSearchEncryptedSecret.Value);
            var token = _GetToken();
            var body = new SearchProgramDativaRequestDto
            {
                ProgramName = $"%{searchRequest.ProgramName}%",
                Start = searchRequest.Start,
                Limit = searchRequest.Limit
            };

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token.AccessToken}");

                var serviceResponse = client.PostAsJsonAsync(_ProgramsSearchUrl, body).Result;
                if (serviceResponse.IsSuccessStatusCode == false)
                {
                    throw new Exception($"Error connecting to the ProgramSearch API: {serviceResponse}");
                }

                try
                {
                    return serviceResponse.Content.ReadAsAsync<List<SearchProgramDativaResponseDto>>().Result;
                }
                catch (Exception ex)
                {
                    throw new Exception("Error reading data from the ProgramSearch API", ex);
                }
            }
        }
        private string _GetProgramsSearchClientId()
        {
            return _IsPipelineVariablesEnabled.Value ? _ConfigurationSettingsHelper.GetConfigValueWithDefault(ConfigKeys.PROGRAMSSEARCHCLIENTID_KEY, "5e9kdecif9k6r7ttetgd4e500t") : BroadcastServiceSystemParameter.ProgramsSearchClientId;
        }
        private string GetProgramsSearchEncryptedSecret()
        {
            return _IsPipelineVariablesEnabled.Value ? _ConfigurationSettingsHelper.GetConfigValueWithDefault(ConfigKeys.PROGRAMSSEARCHENCRYPTEDSECRET_KEY, "OJE8vVrWiuZrou5oVn/uVdCmMSCRf/7vhlBB9Uz9bG/dQkN8WKjS1gXV01ANViI+UvbDSI8XjCs=") : BroadcastServiceSystemParameter.ProgramsSearchEncryptedSecret;
        }
    }
}
