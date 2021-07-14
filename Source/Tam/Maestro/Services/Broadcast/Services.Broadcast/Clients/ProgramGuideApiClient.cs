using Services.Broadcast.Entities.ProgramGuide;
using Services.Broadcast.Helpers;
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
        private const int RESPONSE_NOT_READY_PAUSE_MS = 15000;

        private readonly IAwsCognitoClient _TokenClient;
        private readonly string _ProgramGuideUrl;
        private readonly string _TokenUrl;
        private Lazy<string> _ClientId;
        private Lazy<string> _ClientSecret;
        private Lazy<int> _TimeoutSeconds;
        private readonly IConfigurationSettingsHelper _ConfigurationSettingsHelper;
        private readonly IFeatureToggleHelper _FeatureToggleHelper;
        private Lazy<bool> _IsPipelineVariablesEnabled;      
        public static Lazy<int> RequestElementMaxCount;
        private Lazy<string> encryptedSecret;

        public ProgramGuideApiClient(IAwsCognitoClient tokenClient, IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper)
        {
            _TokenClient = tokenClient;
            _ConfigurationSettingsHelper = configurationSettingsHelper;
            _FeatureToggleHelper = featureToggleHelper;
            _IsPipelineVariablesEnabled = new Lazy<bool>(() => _FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_PIPELINE_VARIABLES));
             encryptedSecret = new Lazy<string>( _GetProgramGuideEncryptedSecret);
            _ClientSecret = new Lazy<string>(()=> EncryptionHelper.DecryptString(encryptedSecret.Value, EncryptionHelper.EncryptionKey));
            _ClientId = new Lazy<string>(_GetClientId);
            _TokenUrl = BroadcastServiceSystemParameter.ProgramGuideTokenUrl;
            _ProgramGuideUrl = BroadcastServiceSystemParameter.ProgramGuideUrl;
            _TimeoutSeconds = new Lazy<int>(_GetTimeoutSeconds);           
            RequestElementMaxCount = new Lazy<int>(_GetRequestElementMaxCount);

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
                id = requestElement.Id,
                startdate = requestElement.StartDate.ToString(BroadcastConstants.DATE_FORMAT_STANDARD),
                enddate = requestElement.EndDate.ToString(BroadcastConstants.DATE_FORMAT_STANDARD),
                station = requestElement.StationCallLetters,
                affiliate = requestElement.NetworkAffiliate,
                daypart = _MapRequestDaypart(requestElement.Daypart)
            };

            return apiRequestElement;
        }

        private GuideApiRequestDaypartDto _MapRequestDaypart(GuideRequestDaypartDto requestDaypart)
        {
            var apiRequestDaypart = new GuideApiRequestDaypartDto
            {
                id = requestDaypart.Id,
                dayparttext = requestDaypart.Name,
                mon = requestDaypart.Monday,
                tue = requestDaypart.Tuesday,
                wed = requestDaypart.Wednesday,
                thu = requestDaypart.Thursday,
                fri = requestDaypart.Friday,
                sat = requestDaypart.Saturday,
                sun = requestDaypart.Sunday,
                starttime = requestDaypart.StartTime,
                endtime = requestDaypart.EndTime
            };

            return apiRequestDaypart;
        }

        private GuideResponseElementDto _MapResponse(GuideApiResponseElementDto apiResponseElement)
        {
            var responseElement = new GuideResponseElementDto
            {
                RequestElementId = apiResponseElement.id,
                RequestDaypartId = apiResponseElement.daypart,
                Station = apiResponseElement.station,
                Affiliate = apiResponseElement.affiliate,
                StartDate = DateTime.Parse(apiResponseElement.start_date),
                EndDate = DateTime.Parse(apiResponseElement.end_date),
                Programs = apiResponseElement.programs.Select(_MapResponseProgram).ToList()
            };

            return responseElement;
        }

        private GuideResponseProgramDto _MapResponseProgram(GuideApiResponseProgramDto apiResponseProgram)
        {
            var responseProgram = new GuideResponseProgramDto
            {
                ProgramName = apiResponseProgram.program,
                SourceGenre = apiResponseProgram.genreid, // genre is the description whereas genreid is the value we map.
                ShowType = apiResponseProgram.showtype,
                SyndicationType = apiResponseProgram.syndicationtype,
                Occurrences = apiResponseProgram.occurances,
                StartDate = apiResponseProgram.startdate,
                EndDate = apiResponseProgram.enddate,
                StartTime = _ConvertTimeStringToSecondsFromMidnight(apiResponseProgram.starttime),
                EndTime = _ConvertTimeStringToSecondsFromMidnight(apiResponseProgram.endtime) - 1, // make it :59
                Monday = apiResponseProgram.mon,
                Tuesday = apiResponseProgram.tue,
                Wednesday = apiResponseProgram.wed,
                Thursday = apiResponseProgram.thu,
                Friday = apiResponseProgram.fri,
                Saturday = apiResponseProgram.sat,
                Sunday = apiResponseProgram.sun
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
            var timeoutTime = DateTime.Now.AddSeconds(_TimeoutSeconds.Value);
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
            return _TokenClient.GetToken(new AwsTokenRequest { TokenUrl = _TokenUrl, ClientId = _ClientId.Value, ClientSecret = _ClientSecret.Value });
        }

        private void _ValidateSettings()
        {
            _ValidateSetting("ProgramGuideClientId", _ClientId.Value);
            _ValidateSetting("ProgramGuideEncryptedSecret", _ClientSecret.Value);
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
            if (requestElements.Count > RequestElementMaxCount.Value)
            {
                throw new InvalidOperationException($"The request element count of {requestElements.Count} exceeds the max acceptable count of {RequestElementMaxCount.Value}.");
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
        private string _GetClientId()
        {
            var clientId = _IsPipelineVariablesEnabled.Value ? _ConfigurationSettingsHelper.GetConfigValueWithDefault(ConfigKeys.ProgramGuideClientId, "5e9kdecif9k6r7ttetgd4e500t") : BroadcastServiceSystemParameter.ProgramGuideClientId;
            return clientId;
        }
        private string _GetProgramGuideEncryptedSecret()
        {
            var programGuideEncyptedSecret = _IsPipelineVariablesEnabled.Value ? _ConfigurationSettingsHelper.GetConfigValueWithDefault(ConfigKeys.ProgramGuideEncryptedSecret, "OJE8vVrWiuZrou5oVn/uVdCmMSCRf/7vhlBB9Uz9bG/dQkN8WKjS1gXV01ANViI+UvbDSI8XjCs=" ) : BroadcastServiceSystemParameter.ProgramGuideEncryptedSecret;
            return programGuideEncyptedSecret;
        }
        private int _GetRequestElementMaxCount()
        {
            var requestElementMaxCount = _IsPipelineVariablesEnabled.Value ? _ConfigurationSettingsHelper.GetConfigValueWithDefault(ConfigKeys.ProgramGuideRequestElementMaxCount, 10) : BroadcastServiceSystemParameter.ProgramGuideRequestElementMaxCount;
            return requestElementMaxCount;
        }
        private int _GetTimeoutSeconds()
        {
            var getTimeoutSeconds = _IsPipelineVariablesEnabled.Value ? _ConfigurationSettingsHelper.GetConfigValueWithDefault(ConfigKeys.ProgramGuideTimeoutSeconds, 1200) : BroadcastServiceSystemParameter.ProgramGuideTimeoutSeconds;
            return getTimeoutSeconds;
        }
    }
}
