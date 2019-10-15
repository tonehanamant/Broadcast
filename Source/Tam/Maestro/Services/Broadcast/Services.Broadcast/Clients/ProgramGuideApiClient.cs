using Services.Broadcast.Entities.ProgramGuide;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using Cadent.Utilities.Clients;
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

        private readonly IAwsCognitoClient _TokenClient;
        private readonly IRestClient _RestClient;

        // TODO: Remove this.  It's for testing.
        private readonly bool _UseRestClient = false;

        public ProgramGuideApiClient(IAwsCognitoClient tokenClient, IRestClient restClient)
        {
            _TokenClient = tokenClient;
            _RestClient = restClient;

            // TODO Get these from the database configuration once they are finalized
            // should these be readonly like this or should they be something else
            // to allow consume a changed configuration value on next attempt?
            _BaseProgramGuideUrl = @"https://virtserver.swaggerhub.com/Cadent7/ProgramGuideAPI/1.0.0";
            _UrlProgramGuides = @"/v1/programs/guide/";
            _UrlProgramsSearch = @"/v1/programs/search/";
            _TokenUrl = @"https://dev-cmw.auth.us-east-1.amazoncognito.com/oauth2/token";
            _ClientId = @"5e9kdecif9k6r7ttetgd4e500t";
            _EncryptedSecret = @"OJE8vVrWiuZrou5oVn/uVdCmMSCRf/7vhlBB9Uz9bG/dQkN8WKjS1gXV01ANViI+UvbDSI8XjCs=";
            _ClientSecret = EncryptionHelper.DecryptString(_EncryptedSecret, EncryptionHelper.EncryptionKey);
        }

        public List<GuideResponseElementDto> GetProgramsForGuide(List<GuideRequestElementDto> requestElements)
        {
            return _Post<List<GuideResponseElementDto>>($"{_BaseProgramGuideUrl}{_UrlProgramGuides}", requestElements);
        }

        public List<SearchResponseProgramDto> GetPrograms()
        {
            return _Post<List<SearchResponseProgramDto>>($"{_BaseProgramGuideUrl}{_UrlProgramsSearch}", null);
        }

        private AwsToken _GetToken()
        {
            return _TokenClient.GetToken(new AwsTokenRequest { TokenUrl = _TokenUrl, ClientId = _ClientId, ClientSecret = _ClientSecret });
        }

        protected virtual T _Post<T>(string url, object data)
        {
            var token = _GetToken();
            T output;
            // TODO: PRI-17014 - change to use the IRestClient 
            // Will complete this in PRI-17014 when we get the actual API.
            if (_UseRestClient)
            {
                /* With RestClient */
                var opts = new RestRequestOptions();
                opts.IncludeHeader(new AuthenticationHeaderValue(BEARER, token.AccessToken));
                //opts.IncludeHeader(new RestHeader(AUTHORIZATION, $"{BEARER} {token.AccessToken}", HeaderTypes.Request));
                //opts.IncludeHeader("Accept", @"application/json", HeaderTypes.Request);
                //var jsonInput = JsonConvert.SerializeObject(data);

                var serviceResponse = _RestClient.PostAsync<T>(url, data, opts).Result;
                // goes in here and never comes out.
                output = serviceResponse.ObjectContent;
            }
            else
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add(AUTHORIZATION, $"{BEARER} {token}");
                    var serviceResponse = client.PostAsJsonAsync(url, data).Result;
                    if (serviceResponse.IsSuccessStatusCode == false)
                    {
                        throw new Exception($"Error connecting to ProgramGuide for post data. : {serviceResponse}");
                    }

                    try
                    {
                        output = serviceResponse.Content.ReadAsAsync<T>().Result;
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Error calling the ProgramGuide for post data.", e);
                    }
                }
            }

            return output;
        }
    }
}