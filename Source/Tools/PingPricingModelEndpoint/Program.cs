using Services.Broadcast.Entities.Plan.Pricing;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace PingPricingModelEndpoint
{
    /// <summary>
    /// Quick tool to that we can communicate with pricing and have a request accepted.
    /// Builds a request and sends it. 
    /// </summary>
    /// <param name="args"></param>
    class Program
    {
        const string testV2UrlProd = @"https://datascience-prod.cadent.tv/broadcast-openmarket-allocations/v2/allocation";
        const string testV2UrlDev = @"https://datascience-dev.cadent.tv/broadcast-openmarket-allocations/v2/allocation";

        static void Main(string[] args)
        {
            var testUrl = testV2UrlDev;

            try
            {
                var worker = new Worker();

                Console.WriteLine($"Attempting call to '{testUrl}'");
                worker.PingPricingModel(testUrl);
                Console.WriteLine("Attempt succeeded.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception caught during test : {e.Message}");
                Console.WriteLine(e);
            }

            Console.WriteLine();
            Console.WriteLine("All done.");
            Console.WriteLine("(Hit [ENTER] to end...)");
            Console.ReadLine();
        }
    }

    public class TestPricingApiClient
    {
        public PlanPricingApiSpotsResponseDto GetPricingSpotsResult(PlanPricingApiRequestDto request, string url)
        {
            return _Post<PlanPricingApiSpotsResponseDto>(url, request);
        }

        protected virtual T _Post<T>(string url, object data)
        {
            T output;
            using (var client = new HttpClient())
            {
                var serviceResponse = client.PostAsJsonAsync(url, data).Result;

                if (serviceResponse.IsSuccessStatusCode == false)
                {
                    try
                    {
                        output = serviceResponse.Content.ReadAsAsync<T>().Result;
                        return output;
                    }
                    catch
                    {
                        throw new Exception($"Error connecting to Pricing API for post data. : {serviceResponse}");
                    }

                }

                try
                {
                    output = serviceResponse.Content.ReadAsAsync<T>().Result;
                }
                catch (Exception e)
                {
                    throw new Exception("Error calling the Pricing API for post data during post-get.", e);
                }
            }

            return output;
        }
    }

    public class Worker
    {
        private PlanPricingApiRequestDto _GetRequest()
        {
            var request = new PlanPricingApiRequestDto
            {
                Weeks = new List<PlanPricingApiRequestWeekDto>
                {
                    new PlanPricingApiRequestWeekDto
                    {
                        MediaWeekId = 852,
                        ImpressionGoal = 10,
                        CpmGoal = 5m,
                        MarketCoverageGoal = .001,
                        FrequencyCapSpots = 1,
                        FrequencyCapTime = 0.5,
                        FrequencyCapUnit = "hour",
                        ShareOfVoice = new List<ShareOfVoice>
                        {
                            new ShareOfVoice
                            {
                                MarketCode = 101,
                                MarketGoal = 0.01
                            }
                        }
                    }
                },
                Spots = new List<PlanPricingApiRequestSpotsDto>
                {
                    new PlanPricingApiRequestSpotsDto
                    {
                        Id = 303705,
                        MediaWeekId = 852,
                        DaypartId = 17,
                        Impressions = 14400,
                        Cost = 5,
                        StationId = 1372,
                        MarketCode = 101,
                        PercentageOfUs = 0.06309000000000001,
                        SpotDays = 1,
                        SpotHours = 0.5
                    },
                    new PlanPricingApiRequestSpotsDto
                    {
                        Id = 303706,
                        MediaWeekId = 852,
                        DaypartId = 17,
                        Impressions = 14400,
                        Cost = 5,
                        StationId = 1373,
                        MarketCode = 101,
                        PercentageOfUs = 0.06309000000000001,
                        SpotDays = 1,
                        SpotHours = 0.5
                    },
                    new PlanPricingApiRequestSpotsDto
                    {
                        Id = 303707,
                        MediaWeekId = 852,
                        DaypartId = 17,
                        Impressions = 14400,
                        Cost = 5,
                        StationId = 1374,
                        MarketCode = 101,
                        PercentageOfUs = 0.06309000000000001,
                        SpotDays = 1,
                        SpotHours = 0.5
                    },
                    new PlanPricingApiRequestSpotsDto
                    {
                        Id = 303708,
                        MediaWeekId = 852,
                        DaypartId = 17,
                        Impressions = 14400,
                        Cost = 5,
                        StationId = 1375,
                        MarketCode = 101,
                        PercentageOfUs = 0.06309000000000001,
                        SpotDays = 1,
                        SpotHours = 0.5
                    },
                    new PlanPricingApiRequestSpotsDto
                    {
                        Id = 303709,
                        MediaWeekId = 852,
                        DaypartId = 17,
                        Impressions = 14400,
                        Cost = 5,
                        StationId = 1376,
                        MarketCode = 101,
                        PercentageOfUs = 0.06309000000000001,
                        SpotDays = 1,
                        SpotHours = 0.5
                    }
                }
            };

            return request;
        }

        public bool PingPricingModel(string testUrl)
        {
            var client = new TestPricingApiClient();
            var request = _GetRequest();
            var results = client.GetPricingSpotsResult(request, testUrl);
            return true;
        }
    }
}
