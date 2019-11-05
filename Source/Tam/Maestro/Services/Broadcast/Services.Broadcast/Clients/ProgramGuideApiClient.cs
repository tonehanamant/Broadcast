using Services.Broadcast.Entities.ProgramGuide;
using System;
using System.Collections.Generic;
using System.Net.Http;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using Tam.Maestro.Common;
using Tam.Maestro.Common.Clients;

namespace Services.Broadcast.Clients
{
    public interface IProgramGuideApiClient
    {
        List<GuideResponseElementDto> GetProgramsForGuide(List<GuideRequestElementDto> requestElements);

        List<SearchResponseProgramDto> GetPrograms();
    }

    public class ProgramGuideApiClient : IProgramGuideApiClient
    {
        private const string AUTHORIZATION = "Authorization";
        private const string BEARER = "Bearer";

        private readonly string _BaseProgramGuideUrl;
        protected readonly string _UrlProgramGuides;
        protected readonly string _UrlProgramsSearch;
        private readonly string _TokenUrl;
        private readonly string _ClientId;
        private readonly string _EncryptedSecret;
        private readonly string _ClientSecret;
        private readonly int _TimeoutSeconds;

        private readonly IAwsCognitoClient _TokenClient;

        // TODO: Remove this.  It's for testing.
        private readonly bool _UseRestClient = false;

        public ProgramGuideApiClient(IAwsCognitoClient tokenClient)
        {
            _TokenClient = tokenClient;

            // TODO Get these from the database configuration once they are finalized
            // should these be readonly like this or should they be something else
            // to allow consume a changed configuration value on next attempt?

            // Pre-Dev url
            //_BaseProgramGuideUrl = @"https://virtserver.swaggerhub.com/Cadent7/ProgramGuideAPI/1.0.0";

            // Staging Url - 401 Unauthorized
            _BaseProgramGuideUrl = @"https://h0ix5d7yhb.execute-api.us-east-1.amazonaws.com/staging";

            _UrlProgramGuides = @"/v1/programs/guide/";
            _UrlProgramsSearch = @"/v1/programs/search/";

            _TokenUrl = @"https://dev-cmw.auth.us-east-1.amazoncognito.com/oauth2/token";
            _ClientId = @"5e9kdecif9k6r7ttetgd4e500t";
            _EncryptedSecret = @"OJE8vVrWiuZrou5oVn/uVdCmMSCRf/7vhlBB9Uz9bG/dQkN8WKjS1gXV01ANViI+UvbDSI8XjCs=";
            _ClientSecret = EncryptionHelper.DecryptString(_EncryptedSecret, EncryptionHelper.EncryptionKey);
            _TimeoutSeconds = 20 * 60;
        }

        public List<GuideResponseElementDto> GetProgramsForGuide(List<GuideRequestElementDto> requestElements)
        {
            return _PostAndGet<List<GuideResponseElementDto>>($"{_BaseProgramGuideUrl}{_UrlProgramGuides}", requestElements);
        }

        public List<SearchResponseProgramDto> GetPrograms()
        {
            var request = new SearchRequestProgramDto
            {
                ProgramName = "zo*",
                //Start = 0,
                //Limit = 20,
                Genres = new List<SearchRequestProgramGenreDto>()
            };

            return _PostAndGet<List<SearchResponseProgramDto>>($"{_BaseProgramGuideUrl}{_UrlProgramsSearch}", request);
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