using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Security.Principal;
using Common.Systems.DataTransferObjects;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.Clients;
using Tam.Maestro.Services.ContractInterfaces;
using Tam.Maestro.Services.ContractInterfaces.Common;


namespace Services.Broadcast.IntegrationTests
{
    public class StubbedSMSClient : ISMSClient
    {
        public event GenericEvent<TAMService, string> UriChanged;
        public event GenericEvent<TAMResource, string> ResourceChanged;
        public string TamEnvironment { get; private set; }
        public ServiceStatus GetStatus()
        {
            throw new System.NotImplementedException();
        }

        public string GetUri<T>(TAMService tamService) where T : ITAMService
        {
            throw new System.NotImplementedException();
        }

        public SmsDbConnectionInfo GetSmsDbConnectionInfo(string pTamResource)
        {
            throw new System.NotImplementedException();
        }

        public string GetResource(string pTamResource)
        {
            //@todo, Temporary switch to check if the CI server is running the tests.
            if (System.Configuration.ConfigurationManager.AppSettings["Environment"] == "Development")
            {
                if (pTamResource == "BroadcastConnectionString")
                {
                    return
                        @"Data Source=devsql.dev.crossmw.com\maestro2_dev;Initial Catalog=broadcast_integration;  Persist Security Info=True;user id=tamservice;pwd=KFqUjr+SjgugpL7h7yeJCg==; Asynchronous Processing=true";
                }
                else if (pTamResource == "BroadcastForecastConnectionString")
                {
                    return
                        @"Data Source=devsql.dev.crossmw.com\maestro2_dev;Initial Catalog=broadcast_forecast_integration;  Persist Security Info=True;user id=tamservice;pwd=KFqUjr+SjgugpL7h7yeJCg==; Asynchronous Processing=true";
                }
            }
            else if(System.Configuration.ConfigurationManager.AppSettings["Environment"] == "Staging")
            {
                if (pTamResource == "BroadcastConnectionString")
                {
                    return
                        @"Data Source=devsql.dev.crossmw.com\maestro2_dev;Initial Catalog=broadcast_integration_staging;  Persist Security Info=True;user id=tamservice;pwd=KFqUjr+SjgugpL7h7yeJCg==; Asynchronous Processing=true";
                }
                else if (pTamResource == "BroadcastForecastConnectionString")
                {
                    return
                        @"Data Source=devsql.dev.crossmw.com\maestro2_dev;Initial Catalog=broadcast_forecast_integration_staging;  Persist Security Info=True;user id=tamservice;pwd=KFqUjr+SjgugpL7h7yeJCg==; Asynchronous Processing=true";
                }
            }
            else if (System.Configuration.ConfigurationManager.AppSettings["Environment"] == "Release")
            {
                if (pTamResource == "BroadcastConnectionString")
                {
                    return
                        @"Data Source=devsql.dev.crossmw.com\maestro2_dev;Initial Catalog=broadcast_integration_codefreeze;  Persist Security Info=True;user id=tamservice;pwd=KFqUjr+SjgugpL7h7yeJCg==; Asynchronous Processing=true";
                }
                else if (pTamResource == "BroadcastForecastConnectionString")
                {
                    return
                        @"Data Source=devsql.dev.crossmw.com\maestro2_dev;Initial Catalog=broadcast_forecast_integration_codefreeze;  Persist Security Info=True;user id=tamservice;pwd=KFqUjr+SjgugpL7h7yeJCg==; Asynchronous Processing=true";
                }
            }
            else if (System.Configuration.ConfigurationManager.AppSettings["Environment"] == "Release_CodeFreeze")
            {
                if (pTamResource == "BroadcastConnectionString")
                {
                    return
                        @"Data Source=devsql.dev.crossmw.com\maestro2_dev;Initial Catalog=broadcast_integration_codefreeze_staging;  Persist Security Info=True;user id=tamservice;pwd=KFqUjr+SjgugpL7h7yeJCg==; Asynchronous Processing=true";
                }
                else if (pTamResource == "BroadcastForecastConnectionString")
                {
                    return
                        @"Data Source=devsql.dev.crossmw.com\maestro2_dev;Initial Catalog=broadcast_forecast_integration_codefreeze_staging;  Persist Security Info=True;user id=tamservice;pwd=KFqUjr+SjgugpL7h7yeJCg==; Asynchronous Processing=true";
                }
            }

            throw new Exception("Un-coded resource: " + pTamResource);
        }

        public string GetSystemComponentParameterValue(string pSystemComponentID, string pSystemParameterID)
        {
            string result = string.Empty;
            switch (pSystemParameterID)
            {
                case "BroadcastMatchingBuffer":
                    result = "120";
                    break;
                case "DaypartCacheSlidingExpirationSeconds":
                    result = "1800";
                    break;
                case "UseDayByDayImpressions":
                    result = "False";
                    break;
                case "WWTV_FtpUsername":
                    result = "broadcast";
                    break;
                case "WWTV_FtpPassword":
                    result = "password";
                    break;
                case "WWTV_FtpHost":
                    result = "cad-phl-mhohens.crossmw.com";
                    break;
                case "WWTV_FtpOutboundFolder":
                    result = "uploads";
                    break;
                case "WWTV_FtpErrorFolder":
                    result = "Errors";
                    break;
                case "WWTV_FailedFolder":
                    result = @"E:\temp";
                    break;
                case "WWTV_FtpInboundFolder":
                    result = @"OutPost";
                    break;
                case "EmailHost": 
                    result = "smtp.office365.com";
                    break;
                case "EmailFrom":
                    result = "broadcast@crossmw.com";
                    break;
                case "EmailUsername":
                    result = "broadcastsmtp@crossmw.com";
                    break;
                case "EmailPassword":
                    result = "7TUCE+HAp3LDexQ6JIvaEA==";
                    break;
                case "EmailWhiteList":
                    result = "mhohenshilt@crossmw.com";
                    break;
                case "WWTV_NotificationEmail":
                    result = "mhohenshilt@crossmw.com";
                    break;
                case "EmailNotificationsEnabled":
                    result = "True";
                    break;
                case "WWTV_LocalFtpErrorFolder":
                    result = ".\\Files";
                    break;
                case "WWTV_SharedFolder_UserName":
                    result = "username";
                    break;
                case "WWTV_SharedFolder_Password":
                    result = "password";
                    break;
                case "DefaultNtiConversionFactor":
                   result = "0.2";
                   break;   
                case "WWTV_SharedFolder":
                    result = "C:\\WWTV\\WWTVData";
                    break;
                case "WWTV_PostLogDropFolder":
                    result = @"E:\temp\wwtv\PostLogPreprocessing";
                    break;
                case "WWTV_PostLogErrorFolder":
                    result = @"E:\temp\wwtv\WWTVErrors\PostLogPreprocessing";
                    break;
                case "WWTV_SpotTrackerDropFolder":
                    result = @"D:\temp\wwtv\SpotTrackerDropFolder";
                    break;
                case "WWTV_PostLogFtpOutboundFolder":
                    result = "InPrePost";
                    break;
                case "MediaMonthCruchCacheSlidingExpirationSeconds":
                    result = "24";
                    break;
                case "WWTV_KeepingTracFtpInboundFolder":
                    result = @"D:\temp\wwtv\WWTV_KeepingTracFtpInboundFolder";
                    break;
                case "ImpressionStartEndTimeAdjustment":
                    result = "420";
                    break;
                case "ImpressionStartOfDayForAdjustment":
                    result = @"18000";
                    break;
                default:
                    throw new Exception("Unknown SystemComponentParameter: " + pSystemParameterID);
            }
            return result;
        }

        public bool ClearSystemComponentParameterCache(string pSystemComponentID, string pSystemParameterID)
        {
            throw new System.NotImplementedException();
        }

        public bool ValidateEnvironment()
        {
            throw new System.NotImplementedException();
        }

        public TAMResult IsLocked(IBusinessEntity[] pEntities)
        {
            return new TAMResult()
            {
                MultipleResult = new object[] {false}
            };
        }

        public TAMResult LockEntity(IBusinessEntity[] pEntities)
        {
            return new TAMResult()
            {
                SingleResult = true
            };
        }

        public void ReleaseEntity(IBusinessEntity[] pEntities)
        {
        }

        public MaestroImage GetLogoImage(CMWImageEnums logoType)
        {
            var image = System.Drawing.Image.FromFile(".\\Penny_test.jpg");
            using (var ms = new MemoryStream())
            {
                image.Save(ms, image.RawFormat);

                return new MaestroImage()
                {
                    ImageData = ms.ToArray()
                };
            }
        }

        public LockResponse LockObject(string key, string userId)
        {
            return new LockResponse
            {
                Success = true
            };
        }

        public ReleaseLockResponse ReleaseObject(string key, string userId)
        {
            return null;
        }

        public bool IsObjectLocked(string key, string userId)
        {
            return false;
        }

        public List<LookupDto> GetActiveAdvertisers()
        {
            return new List<LookupDto>
            {
                new LookupDto(1, "Test Advertiser 1"),
                new LookupDto(2, "Test Advertiser 2"),
                new LookupDto(3, "Test Advertiser 3"),
                new LookupDto(4, "Test Advertiser 4"),
                new LookupDto(37444, "Leagas Delaney"),
                new LookupDto(37674, "1 Vision"),
            };
        }

        public List<LookupDto> FindAdvertisersByIds(List<int> advertiserIds)
        {
            var lookups = new List<LookupDto>();
            int ctr = 5;
            foreach (var advertiserId in advertiserIds)
            {
                lookups.Add(new LookupDto(ctr,"Test Advertiser " + ctr));
                ctr++;
            }
            return lookups;
        }

        public LookupDto FindAdvertiserById(int advertiserId)
        {
            if (advertiserId == 0)
            {
                throw new Exception("Cannot find advertiser");
            }

            return new LookupDto(9, "Test Advertiser 9");
        }
    }
}