﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Services.Broadcast;
using Services.Broadcast.Clients;
using Services.Broadcast.Helpers;
using Services.Broadcast.Entities.Plan.Pricing;

namespace PricingModelEndpointTester
{
    public class PricingJobQueueApiClientTest : ITest
    {
        public async Task<bool> Run()
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

            var client = _GetClient(settings, featureToggle);
            Console.WriteLine("Client loaded.");

            Console.WriteLine("Making the request...");
            var callSw = new Stopwatch();
            callSw.Start();

            // budget : works
            // cpm : not work
            // impressions : works
            request.Configuration.BudgetCpmLever = PlanPricingBudgetCpmLeverEnum.budget;

            var result = await client.GetPricingSpotsResultAsync(request);
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

        private PricingJobQueueApiClient _GetClient(Dictionary<string, object> settingsDict, Dictionary<string, bool> featureToggleDict)
        {
            var csHelper = new TestConfigurationSettingsHelper(settingsDict);
            var feattureToggle = new TestFeatureToggleHelper(featureToggleDict);
            var httpClient = new HttpClient();

            var client = new PricingJobQueueApiClient(csHelper, feattureToggle, httpClient);

            return client;
        }
    }
}