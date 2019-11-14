using Services.Broadcast.Entities.DTO.Program;
using System;
using System.Collections.Generic;
using System.Net.Http;
using Tam.Maestro.Common;
using Tam.Maestro.Common.Clients;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace Services.Broadcast.Clients
{
    public interface IProgramsSearchApiClient
    {
        List<SearchProgramDativaResponseDto> GetPrograms(SearchRequestProgramDto searchRequest);
    }

    public class ProgramsSearchApiClient : IProgramsSearchApiClient
    {
        private readonly string _ProgramsSearchUrl;
        private readonly string _TokenUrl;
        private readonly string _ClientId;
        private readonly string _EncryptedSecret;
        private readonly string _ClientSecret;

        private readonly IAwsCognitoClient _TokenClient;

        public ProgramsSearchApiClient(IAwsCognitoClient tokenClient)
        {
            _TokenClient = tokenClient;

            _ProgramsSearchUrl = BroadcastServiceSystemParameter.ProgramsSearchUrl;
            _TokenUrl = BroadcastServiceSystemParameter.ProgramsSearchTokenUrl;
            _ClientId = BroadcastServiceSystemParameter.ProgramsSearchClientId;
            _EncryptedSecret = BroadcastServiceSystemParameter.ProgramsSearchEncryptedSecret;
            _ClientSecret = EncryptionHelper.DecryptString(_EncryptedSecret, EncryptionHelper.EncryptionKey);
        }
        
        public List<SearchProgramDativaResponseDto> GetPrograms(SearchRequestProgramDto searchRequest)
        {
            var body = new SearchProgramDativaRequestDto
            {
                ProgramName = searchRequest.ProgramName,
                Start = searchRequest.Start,
                Limit = searchRequest.Limit
            };

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_GetToken().AccessToken}");

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

        private AwsToken _GetToken()
        {
            return _TokenClient.GetToken(new AwsTokenRequest { TokenUrl = _TokenUrl, ClientId = _ClientId, ClientSecret = _ClientSecret });
        }
    }
}
