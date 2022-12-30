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
using System.Text.RegularExpressions;
using System.Linq;
using System.Web;

namespace Services.Broadcast.Clients
{
    public interface IInventoryManagementApiClient
    {
        /// <summary>
        /// Get inventory sources for summaries
        /// </summary>
        /// <remarks>
        /// Get a list of inventory sources available for summary
        List<LookupDto> GetInventorySourceTypes();
        /// <summary>
        ///  Get all inventory source types
        /// </summary>
        List<InventorySource> GetInventorySources();
        /// <summary>
        /// Get all quarters for inventory
        /// </summary>
        /// <remarks>
        /// Get a list of quarters for which there is available inventory
        /// 
        /// Make a request without parameters or only with one of the parameters specified 
        /// in order to get a list of quarters for all sources
        /// 
        /// Make a request with both inventorySourceId and standardDaypartId specified in order to get 
        /// a list of quarters for specific inventory source and daypart
        /// </remarks>
        /// <param name="inventorySourceId">Unique identifier of inventory source which is used to filter inventory out</param>
        /// <param name="standardDaypartId">Unique identifier of daypart default which is used to filter inventory out</param>
        InventoryQuartersDto GetInventoryQuarters(int? inventorySourceId = null, int? standardDaypartId = null);
        /// <summary>
        /// Get a list of all available standard dayparts for specific inventory
        /// </summary>
        /// <param name="inventorySourceId">The inventory source identifier.</param>        
        List<StandardDaypartDto> GetStandardDayparts(int inventorySourceId);
        /// <summary>
        /// Get all units for inventory
        /// </summary>
        /// <remarks>
        /// Get a list of units for which there is available inventory that match inventory source, daypart code, start date, end date
        /// </remarks>
        /// <param name="inventorySourceId">Unique identifier of inventory source which is used to filter inventory out</param>
        /// <param name="standardDaypartId">Unique identifier of daypart default which is used to filter inventory out</param>
        /// <param name="startDate">Start date of the period for which inventory needs to be found</param>
        /// <param name="endDate">End date of the period for which inventory needs to be found</param>
        List<string> GetInventoryUnits(int inventorySourceId, int standardDaypartId, DateTime startDate, DateTime endDate);
        List<InventorySummaryDto> GetInventorySummaries(InventorySummaryFilterDto inventorySourceCardFilter);
        /// <summary>Saves an open market inventory file </summary>      
        /// <param name="saveRequest">InventoryFileSaveRequest object containing an open market inventory file</param>
        /// <returns>InventoryFileSaveResult object</returns>
        InventoryFileSaveResult SaveInventoryFile(InventoryFileSaveRequestDto saveRequest);
        /// <summary>
        /// Generates inventory file that contained errors filtered by the id passed
        /// </summary>
        /// <param name="fileId">id to filter the files by</param>
        /// <returns>Returns error file as stream and the file name</returns>
        Tuple<string, Stream, string> DownloadErrorFile(int fileId);
        /// <summary>
        /// Generates inventory file that contained errors filtered by the id passed
        /// </summary>
        /// <param name="inventorySourceId">inventorySourceId to filter record</param>
        /// <param name="quarter">quarter to filter record</param>
        /// <param name="year">year to filter record</param>
        /// <returns>Returns inventory upload history records depending on filter applied</returns>
        List<InventoryUploadHistoryDto> GetInventoryUploadHistory(int inventorySourceId, int? quarter, int? year);
        /// <summary>
        /// Generates an archive with inventory files that contained errors filtered by the list of ids passed
        /// </summary>
        /// <param name="fileIds">List of file ids to filter the files by</param>
        /// <returns>Returns a zip archive as stream and the zip name</returns>
        Tuple<string, Stream> DownloadErrorFiles(List<int> fileIds);
        InventoryQuartersDto GetOpenMarketExportInventoryQuarters(int inventorySourceId);
        List<LookupDto> GetInventoryGenreTypes();
        int GenerateExportForOpenMarket(InventoryExportRequestDto request);
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
            client.Timeout = new TimeSpan(1, 0, 0);
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
        public Tuple<string, Stream, string> DownloadErrorFile(int fileId)
        {
            try
            {
                var requestUri = $"{coreApiVersion}/broadcast/Inventory/DownloadErrorFile?fileId={fileId}";
                var httpClient = _GetSecureHttpClientAsync().GetAwaiter().GetResult();
                var apiResult = httpClient.GetAsync(requestUri).GetAwaiter().GetResult();
                if (apiResult.IsSuccessStatusCode)
                {
                    _LogInfo("Successfully Called the api For download error file api");
                }
                var result = apiResult.Content.ReadAsAsync<ApiItemResponseTyped<InventoryDownloadErrorFileDto>>().Result;
                var rawFileName = result.Result.content.headers[1].value[0].ToString();
                var fileMimeType = result.Result.content.headers[0].value[0].ToString();
                var reg = new Regex("\".*?\"");
                var fileName = reg.Matches(rawFileName)[0].Value.ToString().Replace('"', ' ').Trim();
                string partialPath = @"\InventoryUpload\Errors\";
                string filePath = $"{ _ConfigurationSettingsHelper.GetConfigValue<string>(ConfigKeys.BroadcastAppFolder)}{partialPath}{fileId}_{fileName}";
                 
                Stream stream = new FileStream(filePath, FileMode.Open);
                var errorFile =  new Tuple<string, Stream, string>(fileName, stream, fileMimeType);
                _LogInfo("Successfully get error file: " + JsonConvert.SerializeObject(fileName));
                return errorFile;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(String.Format("Error occured while getting inventory sources, Error:{0}", ex.Message.ToString()));
            }
        }
        public Tuple<string, Stream> DownloadErrorFiles(List<int> fileIds)
        {
            try
            {
                var quertString = _ToQueryString(fileIds);
                var requestUri = $"{coreApiVersion}/broadcast/Inventory/DownloadErrorFiles{quertString}";
                var httpClient = _GetSecureHttpClientAsync().GetAwaiter().GetResult();
                var apiResult = httpClient.GetAsync(requestUri).GetAwaiter().GetResult();
                if (apiResult.IsSuccessStatusCode)
                {
                    _LogInfo("Successfully Called the api For download error files api");
                }
                var result = apiResult.Content.ReadAsAsync<ApiItemResponseTyped<InventoryDownloadErrorFileDto>>().Result;
                var rawFileName = result.Result.content.headers[1].value[0].ToString();
                int pFrom = rawFileName.IndexOf("filename=") + "filename=".Length;
                int pTo = rawFileName.Length;

                string fileName = rawFileName.Substring(pFrom, pTo - pFrom);
                byte[] byteArray = Encoding.UTF8.GetBytes(result.Result.content.ToString());
                MemoryStream stream = new MemoryStream(byteArray);

                var errorFiles = new Tuple<string, Stream>(fileName, stream);
                _LogInfo("Successfully get error file: " + JsonConvert.SerializeObject(fileName));
                return errorFiles;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(String.Format("Error occured while getting inventory sources, Error:{0}", ex.Message.ToString()));
            }
        }
        public List<InventoryUploadHistoryDto> GetInventoryUploadHistory(int inventorySourceId, int? quarter, int? year)
        {
            try
            {
                var requestUri = $"{coreApiVersion}/broadcast/Inventory/UploadHistory?inventorySourceId={inventorySourceId}&quarter={quarter}&year={year}";
                var httpClient = _GetSecureHttpClientAsync().GetAwaiter().GetResult();
                var apiResult = httpClient.GetAsync(requestUri).GetAwaiter().GetResult();
                if (apiResult.IsSuccessStatusCode)
                {
                    _LogInfo("Successfully Called the api For download error file api");
                }
                var result = apiResult.Content.ReadAsAsync<ApiListResponseTyped<InventoryUploadHistoryDto>>();
                var resultList = result.Result.ResultList;
                _LogInfo("Successfully get list of results: " + JsonConvert.SerializeObject(resultList));
                return resultList;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(String.Format("Error occured while getting inventory sources, Error:{0}", ex.Message.ToString()));
            }
        }
        private string _ToQueryString(List<int> fileIds)
        {
            int[] array = fileIds.ToArray();
            var nvc = array.Select(x => new KeyValuePair<string, int>("fileIds", x)).ToList();
            string queryString = null;
            string[] QueryArray = new string[nvc.Count];
            for (int i = 0; i < nvc.Count; i++)
            {
                QueryArray[i] = nvc[i].Key + "=" + nvc[i].Value;
            }
            queryString = "?" + string.Join("&", QueryArray);
            return queryString;
        }

        public InventoryQuartersDto GetOpenMarketExportInventoryQuarters(int inventorySourceId)
        {
            try
            {
                var requestUri = $"{coreApiVersion}/broadcast/InventoryExport/QuartersOpenMarket?inventorySourceId={inventorySourceId}";
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
                throw new InvalidOperationException(String.Format("Error occured while getting inventory Ecport quarters, Error:{0}", ex.Message.ToString()));
            }
        }

        public List<LookupDto> GetInventoryGenreTypes()
        {
            try
            {
                var requestUri = $"{coreApiVersion}/broadcast/InventoryExport/GenreTypes";
                var httpClient = _GetSecureHttpClientAsync().GetAwaiter().GetResult();
                var apiResult = httpClient.GetAsync(requestUri).GetAwaiter().GetResult();
                if (apiResult.IsSuccessStatusCode)
                {
                    _LogInfo("Successfully Called the api For get inventoryExport Genres api");
                }
                var result = apiResult.Content.ReadAsAsync<ApiListResponseTyped<LookupDto>>();
                List<LookupDto> inventoryGeners = result.Result.ResultList;
                _LogInfo("Successfully get inventory sources: " + JsonConvert.SerializeObject(inventoryGeners));
                return inventoryGeners;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(String.Format("Error occured while getting inventory genres, Error:{0}", ex.Message.ToString()));
            }
        }

        public int GenerateExportForOpenMarket(InventoryExportRequestDto request)
        {
            try
            {
                int result1 = 0;
                var requestUri = $"{coreApiVersion}/broadcast/InventoryExport/GenerateExportForOpenMarket";
                var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                var httpClient = _GetSecureHttpClientAsync().GetAwaiter().GetResult();
                var apiResult = httpClient.PostAsync(requestUri, content).GetAwaiter().GetResult();
                if (apiResult.IsSuccessStatusCode)
                {
                    _LogInfo("Successfully Called the api For get inventory summaries api");
                }
                var result = apiResult.Content.ReadAsAsync<ApiItemResponseTyped<int>>();
                result1 = result.Result.Result;

                _LogInfo("Successfully get inventory export for open market");
                return result1;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(string.Format("Error occured while getting inventory summaries, Error:{0}", ex.Message.ToString()));
            }
        }
    }
}
