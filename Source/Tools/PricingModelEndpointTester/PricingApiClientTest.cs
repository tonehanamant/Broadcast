using Services.Broadcast;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities.Plan.Pricing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace PricingModelEndpointTester
{
    public class PricingApiClientTest : ITest
    {
        public async Task<bool> Run()
        {
            var settings = new Dictionary<string, object>();

            settings[ConfigKeys.PlanPricingAllocationsEfficiencyModelUrl] = @"https://datascience-uat.cadent.tv/broadcast-openmarket-allocations/v4/allocation";
            settings[ConfigKeys.PlanPricingAllocationsUrl] = @"https://datascience-qa.cadent.tv/broadcast-openmarket-allocations/v2/allocation";

            const string basePath = @"C:\git\Broadcast\Source\Tools\PricingModelEndpointTester\DataFiles";
            const string requestFileName = "qa_v4-request-639_670_F-20210901_151828.log";
            const string outputFileName = "PricingApiClientTestOutput.json";
            const string sqlOutputFileName = "PricingApiClientTestOutputSql.sql";

            var requestFilePath = Path.Combine(basePath, requestFileName);
            var outputFilePath = Path.Combine(basePath, outputFileName);
            var sqlOutputFilePath = Path.Combine(basePath, sqlOutputFileName);

            Console.WriteLine($"Loading request from : ");
            Console.WriteLine($"'{requestFilePath}'");

            var request = Utilities.GetFromFile<PlanPricingApiRequestDto_v3>(requestFilePath);
            Console.WriteLine("Request loaded.");

            var client = _GetClient(settings);
            Console.WriteLine("Client loaded.");

            Console.WriteLine("Making the request...");
            var callSw = new Stopwatch();
            callSw.Start();
            var result = await client.GetPricingSpotsResultAsync(request);
            callSw.Stop();
            Console.WriteLine($"PricingApiClientTest - Response Received. Duration : '{callSw.ElapsedMilliseconds}'ms");


            Console.WriteLine("Saving the response to file...");
            Utilities.WriteToFile(outputFilePath, result);
            Console.WriteLine($"Results written to file '{outputFilePath}'");

            Console.WriteLine($"Transforming to sql...");
            var sqlResults = Utilities.TransformResultToSql(result, "Vanilla");
            Utilities.WriteStringToFile(sqlOutputFilePath, sqlResults);
            Console.WriteLine($"Sql written tp '{sqlOutputFilePath}'");

            return true;
        }

        private PricingApiClient _GetClient(Dictionary<string, object> settingsDict)
        {
            var csHelper = new TestConfigurationSettingsHelper(settingsDict);
            var httpClient = new HttpClient();
            var featureToggleHelper = new TestFeatureToggleHelper(new Dictionary<string, bool>());

            var client = new PricingApiClient(csHelper, httpClient, featureToggleHelper);

            return client;
        }
    }
}