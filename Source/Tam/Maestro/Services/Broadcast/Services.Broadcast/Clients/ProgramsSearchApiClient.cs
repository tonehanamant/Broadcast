using Services.Broadcast.Entities.DTO.Program;
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

        public ProgramsSearchApiClient(IAwsCognitoClient tokenClient) : base(tokenClient)
        {
            _ProgramsSearchUrl = BroadcastServiceSystemParameter.ProgramsSearchUrl;
            _ConfigureTokenizedClient(BroadcastServiceSystemParameter.ProgramsSearchTokenUrl, 
                BroadcastServiceSystemParameter.ProgramsSearchClientId, 
                BroadcastServiceSystemParameter.ProgramsSearchEncryptedSecret);
        }
        
        public List<SearchProgramDativaResponseDto> GetPrograms(SearchRequestProgramDto searchRequest)
        {
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
    }
}
