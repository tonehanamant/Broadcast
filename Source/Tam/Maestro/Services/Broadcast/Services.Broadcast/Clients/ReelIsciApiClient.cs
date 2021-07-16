using Services.Broadcast.Entities.ReelRosterIscis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace Services.Broadcast.Clients
{
    public interface IReelIsciApiClient
    {
        List<ReelRosterIsciDto> GetReelRosterIscis(DateTime startDate, int numberOfDays);
    }

    public class ReelIsciApiClient : IReelIsciApiClient
    {
        private const string dateStringFormat = "yyyy-MM-dd";
        private const string headerKey_ApiKey = "x-api-key";

        private readonly string _ApiUrl;
        private readonly string _ApiKey;

        public ReelIsciApiClient(ReelIsciApiClientConfig clientConfig)
        {
            _ApiUrl = clientConfig.ApiUrl;
            _ApiKey = clientConfig.ApiKey;
        }

        public List<ReelRosterIsciDto> GetReelRosterIscis(DateTime startDate, int numberOfDays)
        {
            const string apiOperation = @"/submit_reel_roster_query";
            if (numberOfDays < 1)
            {
                numberOfDays = 1;
            }
            var startDateString = startDate.ToString(dateStringFormat);
            var paramString = $"?num_days={numberOfDays}&start_date={startDateString}";
            
            var queryUrl = $"{_ApiUrl}{apiOperation}{paramString}";

            var apiReturnRaw = _PostAndGet(queryUrl, _ApiKey);

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
            // TODO: get from the client pool instead of new-ing up
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add(headerKey_ApiKey, apiKey);

                var serviceResponse = client.GetAsync(url).Result;
                if (serviceResponse.IsSuccessStatusCode == false)
                {
                    throw new Exception($"Error connecting to ReelIsciApi for get. : {serviceResponse}");
                }

                try
                {
                    var rawResult = serviceResponse.Content.ReadAsAsync<ReelRosterIsciResponseEntity>().Result;
                    if (!rawResult.Success)
                    {
                        throw new InvalidOperationException($"Error calling the ReelIsciApi for post data during get : {rawResult.Message}");
                    }

                    result = rawResult.Data;
                }
                catch (Exception e)
                {
                    throw new Exception("Error calling the ReelIsciApi for post data during get.", e);
                }
            }
            return result;
        }
    }
}
