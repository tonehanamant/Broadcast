using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Services.Broadcast;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.Entities.Plan.Buying;

namespace PricingModelEndpointTester
{
    public class PricingJobQueueApiClientTest : ITest
    {
        public HttpClient CreateHttpClient()
        {
            var httpClient = new HttpClient();
            Console.WriteLine($"HttpClient created : ");

            return httpClient;
        }

        public async Task<bool> RunPricingFalse(HttpClient httpClient)
        {
            var settings = new Dictionary<string, object>();
            var featureToggle = new Dictionary<string, bool>();

            settings[ConfigKeys.PlanPricingAllocationsEfficiencyModelSubmitUrl] = @"https://datascience-dev.cadent.tv/broadcast-openmarket-allocations/v4/submit";
            settings[ConfigKeys.PlanPricingAllocationsEfficiencyModelFetchUrl] = @"https://datascience-dev.cadent.tv/broadcast-openmarket-allocations/v4/fetch";

            featureToggle[FeatureToggles.ENABLE_ZIPPED_PRICING] = false;

            const string basePath = @"C:\git\Broadcast\Source\Tools\PricingModelEndpointTester\DataFiles";
            const string requestFileName = "qa_v4-request-639_670_F-20210901_151828.log";

            const string outputFileName = "PricingJobQueueApiClientTestOutput.json";
            const string sqlOutputFileName = "PricingJobQueueApiClientTestOutputSql.sql";

            var requestFilePath = Path.Combine(basePath, requestFileName);
            var outputFilePath = Path.Combine(basePath, outputFileName);
            var sqlOutputFilePath = Path.Combine(basePath, sqlOutputFileName);

            Console.WriteLine($"Loading request from : ");
            Console.WriteLine($"'{requestFilePath}'");

            var request = Utilities.GetFromFile<PlanPricingApiRequestDto_v3>(requestFilePath);
            Console.WriteLine("Request loaded.");

            var clientPricing = _GetPricingClient(settings, featureToggle, httpClient);
            //var clientBuying = _GetBuyingClient(settings, featureToggle, httpClient);
            Console.WriteLine("Pricing Client False loaded.");

            Console.WriteLine("Making the pricing request with false...");
            var callSw = new Stopwatch();
            callSw.Start();

            // budget : works
            // cpm : not work
            // impressions : works
            request.Configuration.BudgetCpmLever = PlanPricingBudgetCpmLeverEnum.budget;
            
            var result = await clientPricing.GetPricingSpotsResultAsync(request);
            callSw.Stop();
            Console.WriteLine($"PricingJobQueueApiClientTest - Response Received. Duration : '{callSw.ElapsedMilliseconds}'ms");

            Console.WriteLine("Response Received.");

            Console.WriteLine("Saving the response to file...");
            Utilities.WriteToFile(outputFilePath, result);
            Console.WriteLine($"Results written to file '{outputFilePath}'");

            Console.WriteLine($"Transforming to sql...");
            var sqlResults = Utilities.TransformResultToSql(result, "JobQueueTest");
            Utilities.WriteStringToFile(sqlOutputFilePath, sqlResults);
            Console.WriteLine($"Sql written tp '{sqlOutputFilePath}'");

            return true;
        }

        public async Task<bool> RunPricingTrue(HttpClient httpClient)
        {
            var settings = new Dictionary<string, object>();
            var featureToggle = new Dictionary<string, bool>();

            settings[ConfigKeys.PlanPricingAllocationsEfficiencyModelSubmitUrl] = @"https://datascience-dev.cadent.tv/broadcast-openmarket-allocations/v4/submit";
            settings[ConfigKeys.PlanPricingAllocationsEfficiencyModelFetchUrl] = @"https://datascience-dev.cadent.tv/broadcast-openmarket-allocations/v4/fetch";

            featureToggle[FeatureToggles.ENABLE_ZIPPED_PRICING] = true;

            const string basePath = @"C:\git\Broadcast\Source\Tools\PricingModelEndpointTester\DataFiles";
            const string requestFileName = "qa_v4-request-639_670_F-20210901_151828.log";

            const string outputFileName = "PricingJobQueueApiClientTestOutput.json";
            const string sqlOutputFileName = "PricingJobQueueApiClientTestOutputSql.sql";

            var requestFilePath = Path.Combine(basePath, requestFileName);
            var outputFilePath = Path.Combine(basePath, outputFileName);
            var sqlOutputFilePath = Path.Combine(basePath, sqlOutputFileName);

            Console.WriteLine($"Loading request from : ");
            Console.WriteLine($"'{requestFilePath}'");

            var request = Utilities.GetFromFile<PlanPricingApiRequestDto_v3>(requestFilePath);
            Console.WriteLine("Request loaded.");

            var clientPricing = _GetPricingClient(settings, featureToggle, httpClient);
            //var clientBuying = _GetBuyingClient(settings, featureToggle, httpClient);
            Console.WriteLine("Pricing Client True loaded.");

            Console.WriteLine("Making the pricing request with true...");
            var callSw = new Stopwatch();
            callSw.Start();

            // budget : works
            // cpm : not work
            // impressions : works
            request.Configuration.BudgetCpmLever = PlanPricingBudgetCpmLeverEnum.budget;

            var result = await clientPricing.GetPricingSpotsResultAsync(request);
            callSw.Stop();
            Console.WriteLine($"PricingJobQueueApiClientTest - Response Received. Duration : '{callSw.ElapsedMilliseconds}'ms");

            Console.WriteLine("Response Received.");

            Console.WriteLine("Saving the response to file...");
            Utilities.WriteToFile(outputFilePath, result);
            Console.WriteLine($"Results written to file '{outputFilePath}'");

            Console.WriteLine($"Transforming to sql...");
            var sqlResults = Utilities.TransformResultToSql(result, "JobQueueTest");
            Utilities.WriteStringToFile(sqlOutputFilePath, sqlResults);
            Console.WriteLine($"Sql written tp '{sqlOutputFilePath}'");

            return true;
        }

        public async Task<bool> RunBuyingTrue(HttpClient httpClient)
        {
            var settings = new Dictionary<string, object>();
            var featureToggle = new Dictionary<string, bool>();

            settings[ConfigKeys.PlanPricingAllocationsEfficiencyModelSubmitUrl] = @"https://datascience-dev.cadent.tv/broadcast-openmarket-allocations/v4/submit";
            settings[ConfigKeys.PlanPricingAllocationsEfficiencyModelFetchUrl] = @"https://datascience-dev.cadent.tv/broadcast-openmarket-allocations/v4/fetch";

            featureToggle[FeatureToggles.ENABLE_ZIPPED_PRICING] = true;

            const string basePath = @"C:\git\Broadcast\Source\Tools\PricingModelEndpointTester\DataFiles";
            const string requestFileName = "dev_v4-request-1171_74_E-20221219_170723.log";

            const string outputFileName = "BuyingJobQueueApiClientTestOutput.json";
            const string sqlOutputFileName = "BuyingJobQueueApiClientTestOutputSql.sql";

            var requestFilePath = Path.Combine(basePath, requestFileName);
            var outputFilePath = Path.Combine(basePath, outputFileName);
            var sqlOutputFilePath = Path.Combine(basePath, sqlOutputFileName);

            Console.WriteLine($"Loading request from : ");
            Console.WriteLine($"'{requestFilePath}'");

            var request = Utilities.GetFromFile<PlanBuyingApiRequestDto_v3>(requestFilePath);
            Console.WriteLine("Request loaded.");

            var clientBuying = _GetBuyingClient(settings, featureToggle, httpClient);
            Console.WriteLine("Buying Client True loaded.");

            Console.WriteLine("Making the buying request with true...");
            var callSw = new Stopwatch();
            callSw.Start();

            // budget : works
            // cpm : not work
            // impressions : works
            request.Configuration.BudgetCpmLever = PlanBuyingBudgetCpmLeverEnum.impressions;

            var result = await clientBuying.GetBuyingSpotsResultAsync(request);
            callSw.Stop();
            Console.WriteLine($"PricingJobQueueApiClientTest - Response Received. Duration : '{callSw.ElapsedMilliseconds}'ms");

            Console.WriteLine("Response Received.");

            Console.WriteLine("Saving the response to file...");
            Utilities.WriteToFile(outputFilePath, result);
            Console.WriteLine($"Results written to file '{outputFilePath}'");

            Console.WriteLine($"Transforming to sql...");
            var sqlResults = Utilities.TransformResultToSql(result, "JobQueueTest");
            Utilities.WriteStringToFile(sqlOutputFilePath, sqlResults);
            Console.WriteLine($"Sql written tp '{sqlOutputFilePath}'");

            return true;
        }

        public async Task<bool> RunBuyingFalse(HttpClient httpClient)
        {
            var settings = new Dictionary<string, object>();
            var featureToggle = new Dictionary<string, bool>();

            settings[ConfigKeys.PlanPricingAllocationsEfficiencyModelSubmitUrl] = @"https://datascience-dev.cadent.tv/broadcast-openmarket-allocations/v4/submit";
            settings[ConfigKeys.PlanPricingAllocationsEfficiencyModelFetchUrl] = @"https://datascience-dev.cadent.tv/broadcast-openmarket-allocations/v4/fetch";

            featureToggle[FeatureToggles.ENABLE_ZIPPED_PRICING] = false;

            const string basePath = @"C:\git\Broadcast\Source\Tools\PricingModelEndpointTester\DataFiles";
            const string requestFileName = "dev_v4-request-1171_74_E-20221219_170723.log";

            const string outputFileName = "BuyingJobQueueApiClientTestOutput.json";
            const string sqlOutputFileName = "BuyingJobQueueApiClientTestOutputSql.sql";

            var requestFilePath = Path.Combine(basePath, requestFileName);
            var outputFilePath = Path.Combine(basePath, outputFileName);
            var sqlOutputFilePath = Path.Combine(basePath, sqlOutputFileName);

            Console.WriteLine($"Loading request from : ");
            Console.WriteLine($"'{requestFilePath}'");

            var request = Utilities.GetFromFile<PlanBuyingApiRequestDto_v3>(requestFilePath);
            Console.WriteLine("Request loaded.");

            var clientBuying = _GetBuyingClient(settings, featureToggle, httpClient);
            Console.WriteLine("Buying Client False loaded.");

            Console.WriteLine("Making the buying request with false...");
            var callSw = new Stopwatch();
            callSw.Start();

            // budget : works
            // cpm : not work
            // impressions : works
            request.Configuration.BudgetCpmLever = PlanBuyingBudgetCpmLeverEnum.impressions;

            var result = await clientBuying.GetBuyingSpotsResultAsync(request);
            callSw.Stop();
            Console.WriteLine($"PricingJobQueueApiClientTest - Response Received. Duration : '{callSw.ElapsedMilliseconds}'ms");

            Console.WriteLine("Response Received.");

            Console.WriteLine("Saving the response to file...");
            Utilities.WriteToFile(outputFilePath, result);
            Console.WriteLine($"Results written to file '{outputFilePath}'");

            Console.WriteLine($"Transforming to sql...");
            var sqlResults = Utilities.TransformResultToSql(result, "JobQueueTest");
            Utilities.WriteStringToFile(sqlOutputFilePath, sqlResults);
            Console.WriteLine($"Sql written tp '{sqlOutputFilePath}'");

            return true;
        }

        private PricingJobQueueApiClient _GetPricingClient(Dictionary<string, object> settingsDict, Dictionary<string, bool> featureToggleDict, HttpClient httpClient)
        {
            var csHelper = new TestConfigurationSettingsHelper(settingsDict);
            var featureToggle = new TestFeatureToggleHelper(featureToggleDict);

            var client = new PricingJobQueueApiClient(csHelper, featureToggle, httpClient);

            return client;
        }

        private PlanBuyingJobQueueApiClient _GetBuyingClient(Dictionary<string, object> settingsDict, Dictionary<string, bool> featureToggleDict, HttpClient httpClient)
        {
            var csHelper = new TestConfigurationSettingsHelper(settingsDict);
            var featureToggle = new TestFeatureToggleHelper(featureToggleDict);

            var client = new PlanBuyingJobQueueApiClient(csHelper, featureToggle, httpClient);

            return client;
        }
    }
}