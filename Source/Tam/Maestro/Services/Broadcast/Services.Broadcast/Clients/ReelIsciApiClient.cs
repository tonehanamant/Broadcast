using Services.Broadcast.Entities.ReelRosterIscis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Tam.Maestro.Common;

namespace Services.Broadcast.Clients
{
    public interface IReelIsciApiClient
    {
        List<ReelRosterIsciDto> GetReelRosterIscis(DateTime startDate, int numberOfDays);
    }

    public class ReelIsciApiClient : IReelIsciApiClient
    {
        public const string ReelIsciApiDateFormat = "yyyy-MM-dd";
        private const string headerKey_ApiKey = "x-api-key";

        private Lazy<string> _ApiUrl;
        private Lazy<string> _ApiKey;

        private readonly IConfigurationSettingsHelper _ConfigurationSettingsHelper;
        private readonly HttpClient _HttpClient;

        public ReelIsciApiClient(IConfigurationSettingsHelper configurationSettingsHelper, HttpClient httpClient)
        {
            _ConfigurationSettingsHelper = configurationSettingsHelper;

            _ApiUrl = new Lazy<string>(_GetApiUrl);
            _ApiKey = new Lazy<string>(_GetApiKey);
            _HttpClient = httpClient;
        }

        public List<ReelRosterIsciDto> GetReelRosterIscis(DateTime startDate, int numberOfDays)
        {
            const string apiOperation = @"/submit_reel_roster_query";
            if (numberOfDays < 1)
            {
                numberOfDays = 1;
            }
            var startDateString = startDate.ToString(ReelIsciApiDateFormat);
            var paramString = $"?num_days={numberOfDays}&start_date={startDateString}";

            var queryUrl = $"{_ApiUrl.Value}{apiOperation}{paramString}";

            var apiReturnRaw = _PostAndGet(queryUrl, _ApiKey.Value);

            var result = apiReturnRaw.Select(r =>
                new ReelRosterIsciDto
                {
                    Isci = r.ISCI_Name,
                    SpotLengthDuration = r.Length,
                    StartDate = r.Start_Date,
                    EndDate = r.End_Date,
                    AdvertiserNames = r.Advertiser
                }).ToList();

            return result;
        }

        protected virtual List<ReelRosterIsciEntity> _PostAndGet(string url, string apiKey)
        {
            List<ReelRosterIsciEntity> result = null;

            try
            {
                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, url);
                httpRequestMessage.Headers.Add(headerKey_ApiKey, apiKey);

                var serviceResponse = _HttpClient.SendAsync(httpRequestMessage).Result;
                if (serviceResponse.IsSuccessStatusCode == false)
                {
                    throw new Exception($"Error connecting to ReelIsciApi for get. : {serviceResponse}");
                }

                var rawResult = serviceResponse.Content.ReadAsAsync<ReelRosterIsciResponseEntity>().Result;
                if (!rawResult.Success)
                {
                    throw new InvalidOperationException($"Error calling the ReelIsciApi for post data during get : {rawResult.Message}");
                }

                result = rawResult.Data;
            }
            catch (Exception e)
            {
                throw new Exception("Error calling the ReelIsciApi for post data during get.", e); ;
            }

            return result;
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
