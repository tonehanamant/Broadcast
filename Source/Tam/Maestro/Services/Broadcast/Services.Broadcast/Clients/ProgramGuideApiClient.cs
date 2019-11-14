using Services.Broadcast.Entities.ProgramGuide;
using System;
using System.Collections.Generic;
using System.Net.Http;
using Tam.Maestro.Common;
using Tam.Maestro.Common.Clients;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace Services.Broadcast.Clients
{
    public interface IProgramGuideApiClient
    {
        List<GuideResponseElementDto> GetProgramsForGuide(List<GuideRequestElementDto> requestElements);
    }

    public class ProgramGuideApiClient : IProgramGuideApiClient
    {
        private const string AUTHORIZATION = "Authorization";
        private const string BEARER = "Bearer";
        
        protected readonly string _ProgramGuidesUrl;
        private readonly string _TokenUrl;
        private readonly string _ClientId;
        private readonly string _EncryptedSecret;
        private readonly string _ClientSecret;
        private readonly int _TimeoutSeconds;

        private readonly IAwsCognitoClient _TokenClient;
        
        public ProgramGuideApiClient(IAwsCognitoClient tokenClient)
        {
            _TokenClient = tokenClient;
            
            _ProgramGuidesUrl = BroadcastServiceSystemParameter.ProgramGuideUrl;
            _TokenUrl = BroadcastServiceSystemParameter.ProgramGuideTokenUrl;
            _ClientId = BroadcastServiceSystemParameter.ProgramGuideClientId;
            _EncryptedSecret = BroadcastServiceSystemParameter.ProgramGuideEncryptedSecret;
            _ClientSecret = EncryptionHelper.DecryptString(_EncryptedSecret, EncryptionHelper.EncryptionKey);
            _TimeoutSeconds = BroadcastServiceSystemParameter.ProgramGuideTimeoutSeconds;
        }

        public List<GuideResponseElementDto> GetProgramsForGuide(List<GuideRequestElementDto> requestElements)
        {
            return _PostAndGet<List<GuideResponseElementDto>>(_ProgramGuidesUrl, requestElements);
        }

        private AwsToken _GetToken()
        {
            return _TokenClient.GetToken(new AwsTokenRequest { TokenUrl = _TokenUrl, ClientId = _ClientId, ClientSecret = _ClientSecret });
        }

        protected virtual T _PostAndGet<T>(string url, object data)
        {
            var timeoutTime = DateTime.Now.AddSeconds(_TimeoutSeconds);
            var token = _GetToken();

            // TODO: PRI-17014 - change to use the IRestClient 
            // Will complete this in PRI-17014 when we get the actual API.
            string queryId;
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add(AUTHORIZATION, $"{BEARER} {token.AccessToken}");
                var serviceResponse = client.PostAsJsonAsync(url, data).Result;

                if (serviceResponse.IsSuccessStatusCode == false)
                {
                    throw new Exception($"Error connecting to ProgramGuide for post data. : {serviceResponse}");
                }
                // this returns a guid now.  Then we want to do a Get with that Guid.
                
                try
                {
                    queryId = serviceResponse.Content.ReadAsStringAsync().Result;
                }
                catch (Exception e)
                {
                    throw new Exception("Error calling the ProgramGuide for post data during post-get.", e);
                }
            }

            var chompedUrl = url.EndsWith(@"/") ? url.Remove((url.Length - 1), 1) : url;
            var queryUrl = $"{chompedUrl}?query_execution_id={queryId}";
            T output;
            do
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add(AUTHORIZATION, $"{BEARER} {token.AccessToken}");
                    //client.DefaultRequestHeaders.Add("query_id", $"queryId");
                    var serviceResponse = client.GetAsync(queryUrl).Result;
                    if (serviceResponse.IsSuccessStatusCode == false)
                    {
                        throw new Exception($"Error connecting to ProgramGuide for post data. : {serviceResponse}");
                    }
                    // this returns a guid now.  Then we want to do a Get with that Guid.
                    // what is the waiting response?
                    try
                    {
                        // SDE : 10/22/2019 - switched to Staging, but it's still returning the mocked response.
                        //var queryReturn = serviceResponse.Content.ReadAsStringAsync().Result;

                        output = serviceResponse.Content.ReadAsAsync<T>().Result;
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Error calling the ProgramGuide for post data during post-get.", e);
                    }
                }

                if (output != null)
                {
                    break;
                }
            } while (timeoutTime.Subtract(DateTime.Now).TotalSeconds > 0);

            // TODO SDE : do this timeout exception better
            if (timeoutTime.Subtract(DateTime.Now).TotalSeconds > 0)
            {
                throw new TimeoutException($"ProgramGuideApi Query Timeout exceeded. TimeoutSeconds : '{_TimeoutSeconds}'");
            }
            return output;
        }
    }
}
