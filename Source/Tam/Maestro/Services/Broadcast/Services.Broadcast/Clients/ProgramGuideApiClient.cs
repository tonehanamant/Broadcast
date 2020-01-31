using Services.Broadcast.Entities.ProgramGuide;
using System;
using System.Collections.Generic;
using System.Linq;
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
    }

    public class ProgramGuideApiClient : IProgramGuideApiClient
    {
        private const string AUTHORIZATION = "Authorization";
        private const string BEARER = "Bearer";
        private const string DATE_FORMAT = "yyyy-MM-dd";
        private const int RESPONSE_NOT_READY_PAUSE_MS = 500;

        private readonly IAwsCognitoClient _TokenClient;
        private readonly string _ProgramGuideUrl;
        private readonly string _TokenUrl;
        private readonly string _ClientId;
        private readonly string _ClientSecret;
        private readonly int _TimeoutSeconds;

        public static int RequestElementMaxCount => BroadcastServiceSystemParameter.ProgramGuideRequestElementMaxCount;

        public ProgramGuideApiClient(IAwsCognitoClient tokenClient)
        {
            _TokenClient = tokenClient;

            var encryptedSecret = BroadcastServiceSystemParameter.ProgramGuideEncryptedSecret;
            _ClientSecret = EncryptionHelper.DecryptString(encryptedSecret, EncryptionHelper.EncryptionKey);
            _ClientId = BroadcastServiceSystemParameter.ProgramGuideClientId;
            _TokenUrl = BroadcastServiceSystemParameter.ProgramGuideTokenUrl;
            _ProgramGuideUrl = BroadcastServiceSystemParameter.ProgramGuideUrl;
            _TimeoutSeconds = BroadcastServiceSystemParameter.ProgramGuideTimeoutSeconds;
        }

        public List<GuideResponseElementDto> GetProgramsForGuide(List<GuideRequestElementDto> requestElements)
        {
            _ValidateSettings();
            _ValidateRequests(requestElements);

            var apiRequests = requestElements.Select(_MapRequest).ToList();
            var apiResult = _PostAndGet($"{_ProgramGuideUrl}", apiRequests);
            var result = apiResult.Select(_MapResponse).ToList();

            return result;
        }
        
        private GuideApiRequestElementDto _MapRequest(GuideRequestElementDto requestElement)
        {
            var apiRequestElement = new GuideApiRequestElementDto
            {
                RequestElementId = requestElement.Id,
                StartDate = requestElement.StartDate.ToString(DATE_FORMAT),
                EndDate = requestElement.EndDate.ToString(DATE_FORMAT),
                StationCallLetters = requestElement.StationCallLetters,
                NetworkAffiliate = requestElement.NetworkAffiliate,
                Daypart = _MapRequestDaypart(requestElement.Daypart)
            };

            return apiRequestElement;
        }

        private GuideApiRequestDaypartDto _MapRequestDaypart(GuideRequestDaypartDto requestDaypart)
        {
            var apiRequestDaypart = new GuideApiRequestDaypartDto
            {
                RequestDaypartId = requestDaypart.Id,
                Daypart = requestDaypart.Name,
                Monday = requestDaypart.Monday,
                Tuesday = requestDaypart.Tuesday,
                Wednesday = requestDaypart.Wednesday,
                Thursday = requestDaypart.Thursday,
                Friday = requestDaypart.Friday,
                Saturday = requestDaypart.Saturday,
                Sunday = requestDaypart.Sunday,
                StartTime = requestDaypart.StartTime,
                EndTime = requestDaypart.EndTime
            };

            return apiRequestDaypart;
        }

        private GuideResponseElementDto _MapResponse(GuideApiResponseElementDto apiResponseElement)
        {
            var responseElement = new GuideResponseElementDto
            {
                RequestElementId = apiResponseElement.RequestElementId,
                RequestDaypartId = apiResponseElement.RequestDaypartId,
                Station = apiResponseElement.Station,
                Affiliate = apiResponseElement.Affiliate,
                StartDate = DateTime.Parse(apiResponseElement.StartDate),
                EndDate = DateTime.Parse(apiResponseElement.EndDate),
                Programs = apiResponseElement.Programs.Select(_MapResponseProgram).ToList()
            };

            return responseElement;
        }

        private GuideResponseProgramDto _MapResponseProgram(GuideApiResponseProgramDto apiResponseProgram)
        {
            var responseProgram = new GuideResponseProgramDto
            {
                ProgramName = apiResponseProgram.ProgramName,
                SourceGenre = apiResponseProgram.Genre,
                ShowType = apiResponseProgram.ShowType,
                SyndicationType = apiResponseProgram.SyndicationType,
                Occurrences = apiResponseProgram.Occurrences,
                StartDate = apiResponseProgram.StartDate,
                EndDate = apiResponseProgram.EndDate,
                StartTime = _ConvertTimeStringToSecondsFromMidnight(apiResponseProgram.StartTimeString),
                EndTime = _ConvertTimeStringToSecondsFromMidnight(apiResponseProgram.EndTimeString) - 1, // make it :59
                Monday = apiResponseProgram.Monday,
                Tuesday = apiResponseProgram.Tuesday,
                Wednesday = apiResponseProgram.Wednesday,
                Thursday = apiResponseProgram.Thursday,
                Friday = apiResponseProgram.Friday,
                Saturday = apiResponseProgram.Saturday,
                Sunday = apiResponseProgram.Sunday
            };

            return responseProgram;
        }

        private int _ConvertTimeStringToSecondsFromMidnight(string timeString)
        {
            var result = 0;
            if (string.IsNullOrWhiteSpace(timeString) == false)
            {
                var parts = timeString.Split(':');
                var hoursAsSeconds = int.Parse(parts[0]) * 60 * 60;
                var minutesAsSeconds = int.Parse(parts[1]) * 60;
                result = hoursAsSeconds + minutesAsSeconds;
            }

            return result;
        }

        protected virtual List<GuideApiResponseElementDto> _PostAndGet(string url, List<GuideApiRequestElementDto> data)
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
            List<GuideApiResponseElementDto> output = null;
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

                        Thread.Sleep(RESPONSE_NOT_READY_PAUSE_MS);
                        continue;
                    }

                    output = serviceResponse.Content.ReadAsAsync<List<GuideApiResponseElementDto>>().Result;
                    keepGoing = false;
                }
            } while (keepGoing);

            return output;
        }

        private AwsToken _GetToken()
        {
            return _TokenClient.GetToken(new AwsTokenRequest { TokenUrl = _TokenUrl, ClientId = _ClientId, ClientSecret = _ClientSecret });
        }

        private void _ValidateSettings()
        {
            _ValidateSetting("ProgramGuideClientId", _ClientId);
            _ValidateSetting("ProgramGuideEncryptedSecret", _ClientSecret);
            _ValidateSetting("ProgramGuideTokenUrl", _TokenUrl);
            _ValidateSetting("ProgramGuideUrl", _ProgramGuideUrl);
        }

        private void _ValidateSetting(string key, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new InvalidOperationException($"Setting '{key}' is not set.");
            }
        }

        private void _ValidateRequests(List<GuideRequestElementDto> requestElements)
        {
            if (requestElements.Count > RequestElementMaxCount)
            {
                throw new InvalidOperationException($"The request element count of {requestElements.Count} exceeds the max acceptable count of {RequestElementMaxCount}.");
            }

            requestElements.ForEach(_ValidateRequest);
        }

        private void _ValidateRequest(GuideRequestElementDto requestElement)
        {
            if (string.IsNullOrEmpty(requestElement.NetworkAffiliate))
            {
                throw new InvalidOperationException($"Bad Request.  Request '{requestElement.Id}' requires a NetworkAffiliate.");
            }
            if (string.IsNullOrEmpty(requestElement.StationCallLetters))
            {
                throw new InvalidOperationException($"Bad Request.  Request '{requestElement.Id}' requires StationCallLetters.");
            }
        }
    }
}
