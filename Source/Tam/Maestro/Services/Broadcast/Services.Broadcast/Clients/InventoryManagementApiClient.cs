using Newtonsoft.Json;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.InventorySummary;
using Services.Broadcast.Entities.Locking;
using Services.Broadcast.Extensions;
using Services.Broadcast.Entities.Inventory;
using Services.Broadcast.Helpers;
using Services.Broadcast.Helpers.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using System.Collections.Generic;
using Services.Broadcast.Exceptions;

namespace Services.Broadcast.Clients
{
    public interface IInventoryManagementApiClient
    {
        List<LookupDto> GetInventorySourceTypes();
        List<InventorySource> GetInventorySources();
        InventoryQuartersDto GetInventoryQuarters(int? inventorySourceId = null, int? standardDaypartId = null);
        List<StandardDaypartDto> GetStandardDayparts(int inventorySourceId);
        List<string> GetInventoryUnits(int inventorySourceId, int standardDaypartId, DateTime startDate, DateTime endDate);
        List<InventorySummaryDto> GetInventorySummaries(InventorySummaryFilterDto inventorySourceCardFilter);
        /// <summary>Saves an open market inventory file </summary>      
        /// <param name="saveRequest">InventoryFileSaveRequest object containing an open market inventory file</param>
        /// <returns>InventoryFileSaveResult object</returns>
        InventoryFileSaveResult SaveInventoryFile(InventoryFileSaveRequestDto saveRequest);

    }
    public class InventoryManagementApiClient : CadentSecuredClientBase, IInventoryManagementApiClient
    {
        private const string coreApiVersion = "api/v1";
        public InventoryManagementApiClient(IApiTokenManager apiTokenManager,
                 IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper)
             : base(apiTokenManager, featureToggleHelper, configurationSettingsHelper)
        {
        }
        private async Task<HttpClient> _GetSecureHttpClientAsync()
        {
            var apiBaseUrl = _GetApiUrl();
            var applicationId = _GetApplicationId();
            var appName = _GetAppName();
            var client = await _GetSecureHttpClientAsync(apiBaseUrl, applicationId, appName);
            return client;
        }
        public List<InventorySource> GetInventorySources()
        {
            try
            {
                var requestUri = $"{coreApiVersion}/broadcast/Inventory/Sources";
                var httpClient = _GetSecureHttpClientAsync().GetAwaiter().GetResult();
                var apiResult = httpClient.GetAsync(requestUri).GetAwaiter().GetResult();
                if (apiResult.IsSuccessStatusCode)
                {
                    _LogInfo("Successfully Called the api For get inventory source api");
                }
                var result = apiResult.Content.ReadAsAsync<ApiListResponseTyped<InventorySource>>();
                List<InventorySource> inventorySources = result.Result.ResultList;
                _LogInfo("Successfully get inventory sources: " + JsonConvert.SerializeObject(inventorySources));
                return inventorySources;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(String.Format("Error occured while getting inventory sources, Error:{0}", ex.Message.ToString()));
            }
        }
        public List<LookupDto> GetInventorySourceTypes()
        {
            try
            {
                var requestUri = $"{coreApiVersion}/broadcast/Inventory/SourceTypes";
                var httpClient = _GetSecureHttpClientAsync().GetAwaiter().GetResult();
                var apiResult = httpClient.GetAsync(requestUri).GetAwaiter().GetResult();
                if (apiResult.IsSuccessStatusCode)
                {
                    _LogInfo("Successfully Called the api For get inventory source types api");
                }
                var result = apiResult.Content.ReadAsAsync<ApiListResponseTyped<LookupDto>>();
                List<LookupDto> inventorySourceTypes = result.Result.ResultList;
                _LogInfo("Successfully get inventory sources: " + JsonConvert.SerializeObject(inventorySourceTypes));
                return inventorySourceTypes;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(String.Format("Error occured while getting inventory source types, Error:{0}", ex.Message.ToString()));
            }
        }

        public InventoryQuartersDto GetInventoryQuarters(int? inventorySourceId = null, int? standardDaypartId = null)
        {
            try
            {
                var requestUri = $"{coreApiVersion}/broadcast/Inventory/Quarters?inventorySourceId={inventorySourceId}&standardDaypartId={standardDaypartId}";
                var httpClient = _GetSecureHttpClientAsync().GetAwaiter().GetResult();
                var apiResult = httpClient.GetAsync(requestUri).GetAwaiter().GetResult();
                if (apiResult.IsSuccessStatusCode)
                {
                    _LogInfo("Successfully Called the api For get inventory quarters api");
                }
                var result = apiResult.Content.ReadAsAsync<ApiItemResponseTyped<InventoryQuartersDto>>();
                InventoryQuartersDto inventoryQuarters = result.Result.Result;
                _LogInfo("Successfully get inventory sources: " + JsonConvert.SerializeObject(inventoryQuarters));
                return inventoryQuarters;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(String.Format("Error occured while getting inventory quarters, Error:{0}", ex.Message.ToString()));
            }
        }

        public List<StandardDaypartDto> GetStandardDayparts(int inventorySourceId)
        {
            try
            {
                var requestUri = $"{coreApiVersion}/broadcast/Inventory/standarddayparts?inventorySourceId={inventorySourceId}";
                var httpClient = _GetSecureHttpClientAsync().GetAwaiter().GetResult();
                var apiResult = httpClient.GetAsync(requestUri).GetAwaiter().GetResult();
                if (apiResult.IsSuccessStatusCode)
                {
                    _LogInfo("Successfully Called the api For get standard dayparts api");
                }
                var result = apiResult.Content.ReadAsAsync<ApiListResponseTyped<StandardDaypartDto>>();
                List<StandardDaypartDto> standardDayparts = result.Result.ResultList;
                _LogInfo("Successfully get standard dayparts: " + JsonConvert.SerializeObject(standardDayparts));
                return standardDayparts;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(String.Format("Error occured while getting standard dayparts, Error:{0}", ex.Message.ToString()));
            }

        }
        public List<string> GetInventoryUnits(int inventorySourceId, int standardDaypartId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var requestUri = $"{coreApiVersion}/broadcast/Inventory/units?inventorySourceId={inventorySourceId}&standardDaypartId={standardDaypartId}&startDate={startDate}&endDate={endDate}";
                var httpClient = _GetSecureHttpClientAsync().GetAwaiter().GetResult();
                var apiResult = httpClient.GetAsync(requestUri).GetAwaiter().GetResult();
                if (apiResult.IsSuccessStatusCode)
                {
                    _LogInfo("Successfully Called the api For get inventory units api");
                }
                var result = apiResult.Content.ReadAsAsync<ApiListResponseTyped<string>>();
                List<string> inventoryUnits = result.Result.ResultList;
                _LogInfo("Successfully get inventory units: " + JsonConvert.SerializeObject(inventoryUnits));
                return inventoryUnits;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(String.Format("Error occured while getting inventory units, Error:{0}", ex.Message.ToString()));
            }
        }

        public List<InventorySummaryDto> GetInventorySummaries(InventorySummaryFilterDto inventorySourceCardFilter)
        {
            try
            {
                List<InventorySummaryDto> inventorySummaries = new List<InventorySummaryDto>();
                List<InventorySummaryApiResponse> inventorySummaryApiResponses = new List<InventorySummaryApiResponse>();
                var requestUri = $"{coreApiVersion}/broadcast/Inventory/Summaries";
                var content = new StringContent(JsonConvert.SerializeObject(inventorySourceCardFilter), Encoding.UTF8, "application/json");
                var httpClient = _GetSecureHttpClientAsync().GetAwaiter().GetResult();
                var apiResult = httpClient.PostAsync(requestUri, content).GetAwaiter().GetResult();
                if (apiResult.IsSuccessStatusCode)
                {
                    _LogInfo("Successfully Called the api For get inventory summaries api");
                }
                var result = apiResult.Content.ReadAsAsync<ApiListResponseTyped<InventorySummaryApiResponse>>();
                inventorySummaryApiResponses = result.Result.ResultList;
                foreach (var items in inventorySummaryApiResponses)
                {
                    InventorySummaryDto item = items;
                    inventorySummaries.Add(item);
                }
                _LogInfo("Successfully get inventory summaries: " + JsonConvert.SerializeObject(inventorySummaries));
                return inventorySummaries;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(string.Format("Error occured while getting inventory summaries, Error:{0}", ex.Message.ToString()));
            }
        }

        private string _GetApiUrl()
        {
            var apiUrl = _ConfigurationSettingsHelper.GetConfigValue<string>(InventoryManagementApiConfigKeys.ApiBaseUrl);

            return apiUrl;
        }
        private string _GetApplicationId()
        {
            var applicationId = _ConfigurationSettingsHelper.GetConfigValue<string>(InventoryManagementApiConfigKeys.ApplicationId);
            return applicationId;
        }
        private string _GetAppName()
        {
            var appName = _ConfigurationSettingsHelper.GetConfigValue<string>(InventoryManagementApiConfigKeys.AppName);
            return appName;
        }

        public InventoryFileSaveResult SaveInventoryFile(InventoryFileSaveRequestDto saveRequest)
        {
            try
            {

                var requestUri = $"{coreApiVersion}/broadcast/Inventory/InventoryFile";
                var httpClient = _GetSecureHttpClientAsync().GetAwaiter().GetResult();

                var content = new StringContent(JsonConvert.SerializeObject(saveRequest), Encoding.UTF8, "application/json");

                var apiResult = httpClient.PostAsync(requestUri, content).Result;                
                var result = apiResult.Content.ReadAsAsync<ApiItemResponseTyped<InventoryFileSaveResult>>();
                if (result.Result.Success)
                {
                    InventoryFileSaveResult inventoryFileSaveResult = new InventoryFileSaveResult
                    {
                        Status = result.Result.Result.Status,
                        FileId = result.Result.Result.FileId,
                        ValidationProblems = result.Result.Result.ValidationProblems
                    };
                    return inventoryFileSaveResult;
                }
                else
                {
                    throw new InvalidOperationException(String.Format(result.Result.Message));
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(String.Format(ex.Message.ToString()));
            }
        }
    }
}
