using Newtonsoft.Json;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.ApplicationServices.Security;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Locking;
using Services.Broadcast.Helpers;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Clients
{
    /// <summary>
    /// Interface for the new general locking microservice methods 
    /// </summary>
    public interface IGeneralLockingApiClient
    {
        /// <summary>
        /// This method is used to create a lock on the objects like campaign, plan etc.
        /// </summary>
        /// <param name="lockingRequest">lockingRequest is passes as paramater</param>
        /// <returns>lock respose</returns>
        LockingResultResponse LockObject(LockingApiRequest lockingRequest);
        /// <summary>
        /// This method is used to release lock on the objects like campaign, plan etc.
        /// </summary>
        /// <param name="objectType">objectType passes as argument</param>
        /// <param name="objectId">objectId passes as agrument</param>
        /// <returns>ReleaseLockResponse returns as response</returns>
        ReleaseLockResponse ReleaseObject(string objectType, string objectId);
        /// <summary>
        /// This method Retrieve active lock status for a specific object type and object ID
        /// </summary>
        /// <param name="objectType">objectType passes as argument</param>
        /// <param name="objectId">objectId passes as agrument</param>
        /// <returns>Returns true if lock exits otherwise false</returns>
        bool IsObjectLocked(string objectType, string objectId);
    }
    /// <summary>
    /// This class contains the new general locking microservice methods 
    /// </summary>
    public class GeneralLockingApiClient : CadentSecuredClientBase, IGeneralLockingApiClient
    {
       
        private readonly BroadcastApplicationServiceFactory _ApplicationServiceFactory;
        public GeneralLockingApiClient(IApiTokenManager apiTokenManager, BroadcastApplicationServiceFactory applicationServiceFactory,
                IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper, HttpClient httpClient
                 )
            : base(apiTokenManager, featureToggleHelper, configurationSettingsHelper)
        {
            _ApplicationServiceFactory = applicationServiceFactory;
        }
        /// <summary>
        /// This method is used to create a lock on the objects like campaign, plan etc.
        /// </summary>
        /// <param name="lockingRequest">lockingRequest is passes as paramater</param>
        /// <returns>lock respose</returns>
        public LockingResultResponse LockObject(LockingApiRequest lockingRequest)
        {
            try
            {
                const string coreApiVersion = "api/v1";
                var requestUri = $"{coreApiVersion}/Lock/set";
                var content = new StringContent(JsonConvert.SerializeObject(lockingRequest), Encoding.UTF8, "application/json");
                var httpClient = _GetSecureHttpClientAsync().GetAwaiter().GetResult();
                var apiResult = httpClient.PostAsync(requestUri, content).GetAwaiter().GetResult();
                if (apiResult.IsSuccessStatusCode)
                {
                    _LogInfo("Successfully Called the api For Locking");
                }
                var result = apiResult.Content.ReadAsAsync<LockingApIItemResponseTyped<LockingResult>>();
                var lockingApiItemResponse = result.Result;
                var lockingApiObjectResponse = result.Result.Result;
                LockingResultResponse lockingResult = new LockingResultResponse()
                {
                    Key = _GetLockingKey(lockingApiObjectResponse.objectType, lockingApiObjectResponse.objectId),                    
                    Success = lockingApiItemResponse.Success,
                    LockTimeoutInSeconds = lockingApiObjectResponse.expiresIn,
                    LockedUserId = lockingApiItemResponse.TransactionId,
                    LockedUserName = lockingApiObjectResponse.owner,
                    Error = lockingApiItemResponse.Message
                };
                return lockingResult;
            }
            catch (Exception ex)
            {
                _LogInfo(String.Format("Error occured while locking object, message:{0}", ex.Message.ToString()));
                LockingResultResponse lockingResult = new LockingResultResponse()
                {
                    Key = null,
                    Success = false,
                    LockTimeoutInSeconds = 0,
                    LockedUserId = null,
                    LockedUserName = null,
                    Error = "Error occured while locking object, message:" + ex.Message.ToString()
                };
                return lockingResult;
            }
        }


        /// <summary>
        /// This method is used to release lock on the objects like campaign, plan etc.
        /// </summary>
        /// <param name="objectType">objectType passes as argument</param>
        /// <param name="objectId">objectId passes as agrument</param>
        /// <returns>ReleaseLockResponse returns as response</returns>
        public ReleaseLockResponse ReleaseObject(string objectType, string objectId)
        {
            try
            {
                const string coreApiVersion = "api/v1";
                var requestUri = $"{coreApiVersion}/Lock/release/{objectType}/{objectId}/";
                var content = new StringContent(JsonConvert.SerializeObject(null), Encoding.UTF8, "application/json");

                var httpClient = _GetSecureHttpClientAsync().GetAwaiter().GetResult();
                var apiResult = httpClient.PostAsync(requestUri, content).GetAwaiter().GetResult();
                if (apiResult.IsSuccessStatusCode)
                {
                    _LogInfo("Successfully Called the api For Release lock");
                }
                var result = apiResult.Content.ReadAsAsync<ReleaseApiResponse>();
                ReleaseLockResponse releaseLock = new ReleaseLockResponse()
                {
                    Error = result.Result.Message,
                    Key = _GetLockingKey(objectType,objectId),
                    Success = result.Result.Success,

                };
                return releaseLock;
            }
            catch (Exception ex)
            {
                _LogInfo(String.Format("Error occured while releasing object, message:{0}", ex.Message.ToString()));
                ReleaseLockResponse releaseLock = new ReleaseLockResponse()
                {
                    Error = "Error occured while releasing object, message:" + ex.Message.ToString(),
                    Key = null,
                    Success = false,

                };
                return releaseLock;
            }
        }
        /// <summary>
        /// This method Retrieve active lock status for a specific object type and object ID
        /// </summary>
        /// <param name="objectType">objectType passes as argument</param>
        /// <param name="objectId">objectId passes as agrument</param>
        /// <returns>Returns true if lock exits otherwise false</returns>
        public bool IsObjectLocked(string objectType, string objectId)
        {
            bool isObjectLocked = false;
            LockingResultResponse lockingResult = ViewLock(objectType, objectId);
            isObjectLocked = lockingResult.Success;
            return isObjectLocked;
        }
        
        private LockingResultResponse ViewLock(string objectType, string objectId)
        {
            try
            {
                const string coreApiVersion = "api/v1";
                var requestUri = $"{coreApiVersion}/Lock/view/{objectType}/{objectId}/";
                var httpClient = _GetSecureHttpClientAsync().GetAwaiter().GetResult();
                var apiResult = httpClient.GetAsync(requestUri).GetAwaiter().GetResult();
                if (apiResult.IsSuccessStatusCode)
                {
                    _LogInfo("Successfully Called the api For view Locking");
                }
                var result = apiResult.Content.ReadAsAsync<LockingApIItemResponseTyped<LockingResult>>();
                var lockingApiItemResponse = result.Result;
                var lockingApiObjectResponse = result.Result.Result;
                var lockingResult = new LockingResultResponse
                {
                    Key = _GetLockingKey(objectType,objectId),
                    Success = lockingApiObjectResponse == null ? false : lockingApiItemResponse.Success,
                    LockedUserId = lockingApiItemResponse.TransactionId,
                    Error = lockingApiItemResponse.Message,
                    LockTimeoutInSeconds = lockingApiObjectResponse != null ? lockingApiObjectResponse.expiresIn : 0,
                    LockedUserName = lockingApiObjectResponse != null ? lockingApiObjectResponse.owner : null
                };
                return lockingResult;
            }
            catch (Exception ex)
            {
                _LogInfo(String.Format("Error occured while checking lock status, Error:{0}", ex.Message.ToString()));
                var lockingResult = new LockingResultResponse
                {
                    Key = null,
                    Success = false,
                    LockedUserId = null,
                    Error = String.Format("Error occured while checking lock status, Error:{0}",ex.Message.ToString()),
                    LockTimeoutInSeconds = 0,
                    LockedUserName = null
                };
                return lockingResult;
            }
        }

        private async Task<HttpClient> _GetSecureHttpClientAsync()
        {
            var apiBaseUrl = _GetApiUrl();
            var applicationId = _GetGeneralLockingApplicationId();
            var appName = _GetGeneralLockingApiAppName();
            var userFullName = _ApplicationServiceFactory.GetApplicationService<IUserService>().GetCurrentUserFullName();
            var client = await _GetSecureHttpClientAsync(apiBaseUrl, applicationId, appName);
            client.DefaultRequestHeaders.Add("owner", userFullName);
            client.Timeout = new TimeSpan(2, 0, 0);
            return client;
        }

        private string _GetApiUrl()
        {
            var apiUrl = _ConfigurationSettingsHelper.GetConfigValue<string>(GeneralLockingApiClientConfigKeys.ApiBaseUrl);
            return apiUrl;
        }
        private string _GetGeneralLockingApplicationId()
        {
            var applicationId = _ConfigurationSettingsHelper.GetConfigValue<string>(GeneralLockingApiClientConfigKeys.ApplicationId);
            return applicationId;
        }
        private string _GetGeneralLockingApiAppName()
        {
            var appName = _ConfigurationSettingsHelper.GetConfigValue<string>(GeneralLockingApiClientConfigKeys.AppName);
            return appName;
        }
        private string _GetLockingKey(string objectType, string objectId)
        {
            return string.Format("{0}:{1}", objectType, objectId);
        }

    }
}
