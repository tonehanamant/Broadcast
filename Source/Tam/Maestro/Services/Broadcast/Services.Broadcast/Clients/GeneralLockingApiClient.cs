using Newtonsoft.Json;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.ApplicationServices.Security;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Locking;
using Services.Broadcast.Helpers;
using System;
using System.Collections.Generic;
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
        /// <summary>
        /// Get the locking request
        /// </summary>
        /// <param name="key">key passes as parameter</param>
        /// <returns>Locking api request</returns>
        LockingApiRequest GetLockingRequest(string key);
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
                LockingResultResponse lockingResult = new LockingResultResponse();                
                if (lockingApiItemResponse != null && lockingApiItemResponse.Success == false)
                {                    
                    lockingResult.Key = _GetLockingKey(lockingRequest.ObjectType, lockingRequest.ObjectId);
                    lockingResult.LockTimeoutInSeconds = 900;
                    lockingResult.LockedUserId = _GetUserName(lockingApiItemResponse.Message);
                    lockingResult.LockedUserName = _GetUserName(lockingApiItemResponse.Message);
                    if (lockingResult.LockedUserId == String.Empty)
                    {
                        _LogInfo("Error occured while locking object, message:" + lockingApiItemResponse.Message);
                        throw new InvalidOperationException(String.Format("Error occured while locking object, message:{0}", lockingApiItemResponse.Message));
                    }
                }
                else
                {
                    lockingResult.Key = _GetLockingKey(lockingApiObjectResponse.objectType, lockingApiObjectResponse.objectId);                   
                    lockingResult.LockTimeoutInSeconds = lockingApiObjectResponse.expiresIn;
                    lockingResult.LockedUserId = lockingApiObjectResponse.owner;
                    lockingResult.LockedUserName = lockingApiObjectResponse.owner;                                   
                }
                lockingResult.Success = lockingApiItemResponse.Success;
                lockingResult.Error = lockingApiItemResponse.Message;                
                _LogInfo("Successfully locked the object with: " + JsonConvert.SerializeObject(lockingRequest));
                return lockingResult;
            }
            catch (Exception ex)
            {                
                throw new InvalidOperationException(String.Format("Error occured while locking object, message:{0}", ex.Message.ToString()));                
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
                _LogInfo("Successfully released locked the object with: " + JsonConvert.SerializeObject(releaseLock));
                return releaseLock;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(String.Format("Error occured while releasing object, message:{0}", ex.Message.ToString()));
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
            _LogInfo("Successfully get locked object information: " + JsonConvert.SerializeObject(lockingResult));
            return isObjectLocked;
        }
        /// <summary>
        /// Get the locking request
        /// </summary>
        /// <param name="key">key passes as parameter</param>
        /// <returns>Locking api request</returns>
        public LockingApiRequest GetLockingRequest(string key)
        {
            string[] lockKeyArray = key.Split(':');
            return new LockingApiRequest
            {
                ObjectType = lockKeyArray[0],
                ObjectId = lockKeyArray[1],
                ExpirationTimeSpan = _GetObjectTimeSpan(lockKeyArray[0]),
                SharedApplications = null,
                IsShared = false
            };
        }
        /// <summary>
        /// Read the object setting from appsetting.json file and set the default time expiration for each locking object 
        /// as  defined in the file
        /// </summary>
        /// <param name="objectType">Object type which is going to lock</param>
        /// <returns>Lock expiration time in minutes</returns>
        private TimeSpan _GetObjectTimeSpan(string objectType)
        {
            try
            {
                _LogInfo("Reading information from appsettings.json file.");
                var objectTypeSettings = _ConfigurationSettingsHelper.GetConfigValue<string>(GeneralLockingApiClientConfigKeys.ObjectTypeSettings);                
                var result = JsonConvert.DeserializeObject<ObjectTypeSettings>(objectTypeSettings);
                string expirationTimeSpan;
                switch (objectType.Trim())
                {
                    case "broadcast_campaign":                        
                        expirationTimeSpan = result.broadcast_campaign.DefaultExpiration;
                        break;
                    case "broadcast_proposal":
                        expirationTimeSpan = result.broadcast_proposal.DefaultExpiration;
                        break;
                    case "broadcast_station":
                        expirationTimeSpan = result.broadcast_station.DefaultExpiration;
                        break;
                    case "broadcast_plan":
                        expirationTimeSpan = result.broadcast_plan.DefaultExpiration;
                        break;
                    default:
                        expirationTimeSpan = "00:15:00";
                        break;
                }
                _LogInfo("Successfully read information from appsettings.json file.");
                return System.TimeSpan.FromMinutes(_GetMinutesFromTime(expirationTimeSpan));
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(String.Format("Error occured while reading lock time information from appsettings.json file, Error:{0}", ex.Message.ToString()));
            }
        }
        private int _GetMinutesFromTime(string expirationTimeSpan)
        {            
            string[] arrTime = expirationTimeSpan.Split(':');            
            return (Convert.ToInt32(arrTime[0]) * 60) + Convert.ToInt32(arrTime[1]) + (Convert.ToInt32(arrTime[2]) / 60);            
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
                LockingResultResponse lockingResult = new LockingResultResponse();
                //If the lock does not exist, then a standard HTTP 200 response will be returned with an empty "result" node
                if (lockingApiObjectResponse == null)
                {
                    lockingResult.Key = _GetLockingKey(objectType, objectId);                    
                    lockingResult.LockTimeoutInSeconds = 0;
                    lockingResult.LockedUserId = null;
                    lockingResult.LockedUserName = null;                    
                }
                else
                {
                    lockingResult.Key = _GetLockingKey(lockingApiObjectResponse.objectType, lockingApiObjectResponse.objectId);                    
                    lockingResult.LockTimeoutInSeconds = lockingApiObjectResponse.expiresIn;
                    lockingResult.LockedUserId = lockingApiObjectResponse.owner;
                    lockingResult.LockedUserName = lockingApiObjectResponse.owner;                   
                }
                lockingResult.Success = lockingApiItemResponse.Success;
                lockingResult.Error = lockingApiItemResponse.Message;
                _LogInfo("Successfully get locked object information: " + JsonConvert.SerializeObject(lockingResult));
                return lockingResult;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(String.Format("Error occured while checking lock status, Error:{0}", ex.Message.ToString()));
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
        private string _GetUserName(string errorMessage)
        {
            string toBeSearched = "user";
            string userName = String.Empty; 
            int ix = errorMessage.IndexOf(toBeSearched);
            if (ix != -1)
            {
                userName = errorMessage.Substring(ix + toBeSearched.Length).Replace(@"'",string.Empty).Trim();                
            }
            return userName;
        }

    }
}
