using Services.Broadcast.Entities.ProgramGuide;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using Tam.Maestro.Common;
using Tam.Maestro.Common.Clients;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace Services.Broadcast.Clients
{
    public interface IProgramGuideApiClient
    {
        List<GuideResponseElementDto> GetProgramsForGuide(List<GuideRequestElementDto> requestElements);

        // TODO: PRI-17014 : Remove this. It's only for POC and testing
        string GetProgramsForGuideAsString(List<GuideRequestElementDto> requestElements);
    }

    public class ProgramGuideApiClient : IProgramGuideApiClient
    {
        private const string AUTHORIZATION = "Authorization";
        private const string BEARER = "Bearer";

        protected readonly string _UrlProgramGuides;
        protected readonly string _ProgramGuidesUrlBase;
        private readonly string _TokenUrl;
        private readonly string _ClientId;
        private readonly string _EncryptedSecret;
        private readonly string _ClientSecret;
        private readonly int _TimeoutSeconds;
        private readonly int _ResponseNotReadyPauseMs;

        private readonly IAwsCognitoClient _TokenClient;
        
        public ProgramGuideApiClient(IAwsCognitoClient tokenClient)
        {
            _UrlProgramGuides = @"/v1/programs/guide/";

            _TokenClient = tokenClient;

            // Dev url
            _ProgramGuidesUrlBase = @"https://qye2zoq6d0.execute-api.us-east-1.amazonaws.com/dev-abr";
            
            // Staging Url 
            //_ProgramGuidesUrlBase = @"https://h0ix5d7yhb.execute-api.us-east-1.amazonaws.com/staging";

            //_ProgramGuidesUrlBase = BroadcastServiceSystemParameter.ProgramGuideUrl;
            _TokenUrl = BroadcastServiceSystemParameter.ProgramGuideTokenUrl;
            _ClientId = BroadcastServiceSystemParameter.ProgramGuideClientId;
            _EncryptedSecret = BroadcastServiceSystemParameter.ProgramGuideEncryptedSecret;
            _ClientSecret = EncryptionHelper.DecryptString(_EncryptedSecret, EncryptionHelper.EncryptionKey);
            _TimeoutSeconds = BroadcastServiceSystemParameter.ProgramGuideTimeoutSeconds;
        }

        public List<GuideResponseElementDto> GetProgramsForGuide(List<GuideRequestElementDto> requestElements)
        {
            return _PostAndGet<List<GuideResponseElementDto>>($"{_ProgramGuidesUrlBase}{_UrlProgramGuides}", requestElements);
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
            string queryResponse;
            var keepGoing = true;
            T output = default(T);
            do
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add(AUTHORIZATION, $"{BEARER} {token.AccessToken}");
                    var serviceResponse = client.GetAsync(queryUrl).Result;
                    if (serviceResponse.IsSuccessStatusCode == false)
                    {
                        throw new Exception($"Error connecting to ProgramGuide for post data. : {serviceResponse}");
                    }
                    try
                    {
                        queryResponse = serviceResponse.Content.ReadAsStringAsync().Result;
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Error calling the ProgramGuide for post data during post-get.", e);
                    }

                    if (queryResponse.Equals("Query not yet completed")
                        || string.IsNullOrWhiteSpace(queryResponse))
                    {
                        if ((timeoutTime.Subtract(DateTime.Now).TotalSeconds) < 0)
                        {
                            throw new TimeoutException($"ProgramGuideApi Query Timeout exceeded. TimeoutSeconds : '{_TimeoutSeconds}'");
                        }

                        Thread.Sleep(_ResponseNotReadyPauseMs);
                        continue;
                    }
                    // if we're here then we have a value
                    output = serviceResponse.Content.ReadAsAsync<T>().Result;
                    keepGoing = false;
                }
            } while (keepGoing);

            return output;
        }

        // TODO: PRI-17014 : Remove this. It's only for POC and testing
        public string GetProgramsForGuideAsString(List<GuideRequestElementDto> requestElements)
        {
            return _PostAndGetAsString($"{_ProgramGuidesUrlBase}{_UrlProgramGuides}", requestElements);
        }

        // TODO: PRI-17014 : Remove this. It's only for POC and testing
        protected string _PostAndGetAsString(string url, object data)
        {
            var timeoutTime = DateTime.Now.AddSeconds(_TimeoutSeconds);
            var token = _GetToken();

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
            string output;
            var keepGoing = true;
            do
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add(AUTHORIZATION, $"{BEARER} {token.AccessToken}");
                    var serviceResponse = client.GetAsync(queryUrl).Result;
                    if (serviceResponse.IsSuccessStatusCode == false)
                    {
                        throw new Exception($"Error connecting to ProgramGuide for post data. : {serviceResponse}");
                    }
                    try
                    {
                        output = serviceResponse.Content.ReadAsStringAsync().Result;
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Error calling the ProgramGuide for post data during post-get.", e);
                    }

                    if (output.Equals("Query not yet completed")
                        || string.IsNullOrWhiteSpace(output))
                    {
                        if ((timeoutTime.Subtract(DateTime.Now).TotalSeconds) < 0)
                        {
                            throw new TimeoutException($"ProgramGuideApi Query Timeout exceeded. TimeoutSeconds : '{_TimeoutSeconds}'");
                        }

                        Thread.Sleep(_ResponseNotReadyPauseMs);
                        continue;
                    }
                    keepGoing = false;
                }
            } while (keepGoing);

            return output;
        }
    }
}
