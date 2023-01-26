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
using Services.Broadcast.Entities.Scx;
using Services.Broadcast.Entities.InventoryMarketAffiliates;
namespace Services.Broadcast.Clients
{
    public interface IInventoryManagementApiClient
    {
        /// <summary>
        /// Get inventory sources for summaries
        /// </summary>
        /// <remarks>
        /// Get a list of inventory sources available for summary
        /// </remarks>
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
        List<InventorySummaryApiResponse> GetInventorySummaries(InventorySummaryFilterDto inventorySourceCardFilter);
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

        /// <summary>
        /// Gets the inventory Quarters
        /// </summary>
        /// <param name="inventorySourceId">source id</param>
        /// <returns>Quarters</returns>
        InventoryQuartersDto GetOpenMarketExportInventoryQuarters(int inventorySourceId);

        /// <summary>
        /// get the inventory geners
        /// </summary>
        /// <returns>geners</returns>
        List<LookupDto> GetInventoryGenreTypes();
        int GenerateExportForOpenMarket(InventoryExportRequestDto request);
        /// <summary>
        /// Generates list of result for inventory source id
        /// </summary>
        /// <param name="sourceId">inventory source id</param>
        /// <returns>Returns a list of scx file generation history</returns>
        List<ScxFileGenerationDetail> GetScxFileGenerationHistory(int sourceId);
        /// <summary>
        /// Generates scx file
        /// </summary>
        /// <param name="fileId">inventory source id</param>
        /// <returns>Returns scx file</returns>
        Tuple<string, Stream, string> DownloadGeneratedScxFile(int fileId);
        List<QuarterDetailDto> GetInventoryUploadHistoryQuarters(int inventorySourceId);

        /// <summary>
        /// Download the inventory for open market
        /// </summary>
        /// <param name="fileId">file id</param>
        /// <returns>Open market file</returns>
        Tuple<string, Stream, string> DownloadInventoeyForOpenMarket(int fileId);

        /// <summary>
        /// Generate the open Market Affiliates
        /// </summary>
        /// <param name="request">quarter and source id</param>
        /// <returns>open market report</returns>
        Guid GenerateOpenMarketAffiliates(InventoryMarketAffiliatesRequest request);

        /// <summary>
        /// process scx download job
        /// </summary>
        /// <param name="jobId">scx download job id</param>
        /// <returns></returns>
        void ProcessScxGenerationJob(int jobId);
        /// <summary>
        /// Process Scx Open Market Job.
        /// </summary>
        /// <param name="jobId">The source identifier.</param>
        /// <returns></returns>
        void ProcessScxOpenMarketGenerationJob(int jobId);
        /// <summary>
        /// Gets the open market History Details
        /// </summary>
        /// <returns>Open market History</returns>
        List<ScxOpenMarketFileGenerationDetail> GetOpenMarketScxFileGenerationHistory();
        /// <summary>
        /// Download the open market scx file
        /// </summary>
        /// <param name="fileId">file id</param>
        /// <returns></returns>
        Tuple<string, Stream, string> DownloadGeneratedScxFileForOpenMarket(int fileId);
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

        public List<QuarterDetailDto> GetInventoryUploadHistoryQuarters(int inventorySourceId)
        {
                try
                {
                    var requestUri = $"{coreApiVersion}/broadcast/Inventory/UploadHistoryQuarters?inventorySourceId={inventorySourceId}";
                    var httpClient = _GetSecureHttpClientAsync().GetAwaiter().GetResult();
                    var apiResult = httpClient.GetAsync(requestUri).GetAwaiter().GetResult();
                    if (apiResult.IsSuccessStatusCode)
                    {
                        _LogInfo("Successfully Called the api For get inventory upload history quarter api");
                    }
                    var result = apiResult.Content.ReadAsAsync<ApiListResponseTyped<QuarterDetailDto>>();
                    List<QuarterDetailDto> InventoryUploadhistoryQuarters = result.Result.ResultList;
                    _LogInfo("Successfully get inventory sources: " + JsonConvert.SerializeObject(InventoryUploadhistoryQuarters));
                    return InventoryUploadhistoryQuarters;
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
        /// <summary>
        /// Get list of inventory summaries
        /// </summary>
        public List<InventorySummaryApiResponse> GetInventorySummaries(InventorySummaryFilterDto inventorySourceCardFilter)
        {
            try
            {
                List<InventorySummaryApiResponse> inventorySummaryApiResponses;
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
                _LogInfo("Successfully get inventory summaries: " + JsonConvert.SerializeObject(inventorySummaryApiResponses));
                return inventorySummaryApiResponses;
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

        public List<ScxFileGenerationDetail> GetScxFileGenerationHistory(int sourceId)
        {
            try
            {
                var requestUri = $"{coreApiVersion}/broadcast/Inventory/ScxFileGenerationHistory?inventorySourceId={sourceId}";
                var httpClient = _GetSecureHttpClientAsync().GetAwaiter().GetResult();
                var apiResult = httpClient.GetAsync(requestUri).GetAwaiter().GetResult();
                if (apiResult.IsSuccessStatusCode)
                {
                    _LogInfo("Successfully Called the api For SCX file Genedration history");
                }
                var result = apiResult.Content.ReadAsAsync<ApiListResponseTyped<ScxFileGenerationDetail>>();
                var resultList = result.Result.ResultList;
                _LogInfo("Successfully get list of SCX file Genedration history: " + JsonConvert.SerializeObject(resultList));
                return resultList;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(String.Format("Error occured while getting scx file generation history, Error:{0}", ex.Message.ToString()));
            }
        }
        public Tuple<string, Stream, string> DownloadGeneratedScxFile(int fileId)
        {
            try
            {
                var requestUri = $"{coreApiVersion}/broadcast/Inventory/DownloadScxFile?fileId={fileId}";
                var httpClient = _GetSecureHttpClientAsync().GetAwaiter().GetResult();
                var apiResult = httpClient.GetAsync(requestUri).GetAwaiter().GetResult();
                if (apiResult.IsSuccessStatusCode)
                {
                    _LogInfo("Successfully Called the api For download scx file");
                }
                var result = apiResult.Content.ReadAsAsync<ApiItemResponseTyped<InventoryDownloadErrorFileDto>>().Result;
                var rawFileName = result.Result.content.headers[1].value[0].ToString();
                var fileMimeType = result.Result.content.headers[0].value[0].ToString();
                var reg = new Regex("\".*?\"");
                var fileName = reg.Matches(rawFileName)[0].Value.ToString().Replace('"', ' ').Trim();
                string partialPath = @"\ScxFiles\";
                string filePath = $"{ _ConfigurationSettingsHelper.GetConfigValue<string>(ConfigKeys.BroadcastAppFolder)}{partialPath}{fileName}";

                Stream stream = new FileStream(filePath, FileMode.Open);
                var errorFile = new Tuple<string, Stream, string>(fileName, stream, fileMimeType);
                _LogInfo("Successfully get scx file: " + JsonConvert.SerializeObject(fileName));
                return errorFile;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(String.Format("Error occured while downloading scx file, Error:{0}", ex.Message.ToString()));
            }
        }

        public Tuple<string, Stream, string> DownloadInventoeyForOpenMarket(int fileId)
        {
            try
            {
                var requestUri = $"{coreApiVersion}/broadcast/InventoryExport/DownloadInventoryExportFile?fileId={fileId}";
                var httpClient = _GetSecureHttpClientAsync().GetAwaiter().GetResult();
                var apiResult = httpClient.GetAsync(requestUri).GetAwaiter().GetResult();
                if (apiResult.IsSuccessStatusCode)
                {
                    _LogInfo("Successfully Called the api For download Inventory for Open Market file");
                }
                var result = apiResult.Content.ReadAsAsync<ApiItemResponseTyped<InventoryDownloadErrorFileDto>>().Result;
                var rawFileName = result.Result.content.headers[1].value[0].ToString();
                var fileMimeType = result.Result.content.headers[0].value[0].ToString();
                var reg = new Regex("\".*?\"");
                var fileName = reg.Matches(rawFileName)[0].Value.ToString().Replace('"', ' ').Trim();
                string partialPath = @"\ScxFiles\";
                string filePath = $"{ _ConfigurationSettingsHelper.GetConfigValue<string>(ConfigKeys.BroadcastAppFolder)}{partialPath}{fileName}";

                Stream stream = new FileStream(filePath, FileMode.Open);
                var errorFile = new Tuple<string, Stream, string>(fileName, stream, fileMimeType);
                _LogInfo("Successfully get file: " + JsonConvert.SerializeObject(fileName));
                return errorFile;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(String.Format("Error occured while downloading scx file, Error:{0}", ex.Message.ToString()));
            }
        }

        public Guid GenerateOpenMarketAffiliates(InventoryMarketAffiliatesRequest request)
        {
            try
            {
                var requestUri = $"{coreApiVersion}/broadcast/InventoryExport/GenerateMarketAffiliatesReport";
                var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                var httpClient = _GetSecureHttpClientAsync().GetAwaiter().GetResult();
                var apiResult = httpClient.PostAsync(requestUri, content).GetAwaiter().GetResult();
                if (apiResult.IsSuccessStatusCode)
                {
                    _LogInfo("Successfully Called the api for Generate Open Market Affiliates report");
                }
                
                var result = apiResult.Content.ReadAsAsync<InventoryExportApiResponse>();                
                Guid affiliateReport =new Guid(result.Result.Data);

                _LogInfo("Successfully get Generate Open Market Affiliates report");
                return affiliateReport;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(string.Format("Error occured while Generating Open Market Affiliates report, Error:{0}", ex.Message.ToString()));
            }
        }

        public void ProcessScxGenerationJob( int jobId)
        {
            try
            {
                var requestUri = $"{coreApiVersion}/broadcast/Inventory/ScxDownloadJob?jobId={jobId}";
                var httpClient = _GetSecureHttpClientAsync().GetAwaiter().GetResult();
                var apiResult = httpClient.GetAsync(requestUri).GetAwaiter().GetResult();
                if (apiResult.IsSuccessStatusCode)
                {
                    _LogInfo("Successfully Called the api scx download job");
                }
               
                _LogInfo("Successfully processed scx download job");
             
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(String.Format("Error occured while processing scx download job, Error:{0}", ex.Message.ToString()));
            }
        }
        
        public void ProcessScxOpenMarketGenerationJob(int jobId)
        {
            try
            {
                var requestUri = $"{coreApiVersion}/broadcast/Inventory/ScxDownloadJob-OpenMarkets?jobId={jobId}";
                var httpClient = _GetSecureHttpClientAsync().GetAwaiter().GetResult();
                var apiResult = httpClient.GetAsync(requestUri).GetAwaiter().GetResult();
                if (apiResult.IsSuccessStatusCode)
                {
                    _LogInfo("Successfully Called the api scx open market download job");
                }

                _LogInfo("Successfully processed scx open market download job");

            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(String.Format("Error occured while processing scx open market download job, Error:{0}", ex.Message.ToString()));
            }
        }
        public List<ScxOpenMarketFileGenerationDetail> GetOpenMarketScxFileGenerationHistory()
        {
            try
            {
                var requestUri = $"{coreApiVersion}/broadcast/Inventory/ScxOpenMarketFileGenerationHistory";
                var httpClient = _GetSecureHttpClientAsync().GetAwaiter().GetResult();
                var apiResult = httpClient.GetAsync(requestUri).GetAwaiter().GetResult();
                if (apiResult.IsSuccessStatusCode)
                {
                    _LogInfo("Successfully Called the api For SCX open market file Genedration history");
                }
                var result = apiResult.Content.ReadAsAsync<ApiListResponseTyped<ScxOpenMarketFileGenerationDetail>>();
                var resultList = result.Result.ResultList;
                _LogInfo("Successfully get list of SCX open market file Genedration history: " + JsonConvert.SerializeObject(resultList));
                return resultList;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(String.Format("Error occured while getting scx open market file generation history, Error:{0}", ex.Message.ToString()));
            }
        }
        public Tuple<string, Stream, string> DownloadGeneratedScxFileForOpenMarket(int fileId)
        {
            try
            {
                var requestUri = $"{coreApiVersion}/broadcast/Inventory/DownloadOpenMarketScxFile?fileId={fileId}";
                var httpClient = _GetSecureHttpClientAsync().GetAwaiter().GetResult();
                var apiResult = httpClient.GetAsync(requestUri).GetAwaiter().GetResult();
                if (apiResult.IsSuccessStatusCode)
                {
                    _LogInfo("Successfully Called the api For download scx open market file");
                }
                var result = apiResult.Content.ReadAsAsync<ApiItemResponseTyped<InventoryDownloadErrorFileDto>>().Result;
                var rawFileName = result.Result.content.headers[1].value[0].ToString();
                var fileMimeType = result.Result.content.headers[0].value[0].ToString();
                var reg = new Regex("\".*?\"");
                var fileName = reg.Matches(rawFileName)[0].Value.ToString().Replace('"', ' ').Trim();
                string partialPath = @"\ScxFiles\";
                string filePath = $"{ _ConfigurationSettingsHelper.GetConfigValue<string>(ConfigKeys.BroadcastAppFolder)}{partialPath}{fileName}";

                Stream stream = new FileStream(filePath, FileMode.Open);
                var errorFile = new Tuple<string, Stream, string>(fileName, stream, fileMimeType);
                _LogInfo("Successfully get scx open market file: " + JsonConvert.SerializeObject(fileName));
                return errorFile;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(String.Format("Error occured while downloading scx open market file, Error:{0}", ex.Message.ToString()));
            }
        }
    }
}
