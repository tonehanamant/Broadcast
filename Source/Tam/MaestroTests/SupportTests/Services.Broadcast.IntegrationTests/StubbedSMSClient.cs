using System;
using System.Collections.Generic;
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
            if (WindowsIdentity.GetCurrent().Owner.Value == "S-1-5-32-544")
            {
                if (pTamResource == "BroadcastConnectionString")
                {
                    return
                        @"Data Source=DEVFSQL2014\MAESTRO2_DEV;Initial Catalog=broadcast_integration;  Persist Security Info=True;user id=tamservice;pwd=KFqUjr+SjgugpL7h7yeJCg==; Asynchronous Processing=true";
                }
                else if (pTamResource == "BroadcastForecastConnectionString")
                {
                    return
                        @"Data Source=DEVFSQL2014\MAESTRO2_DEV;Initial Catalog=broadcast_forecast_integration;  Persist Security Info=True;user id=tamservice;pwd=KFqUjr+SjgugpL7h7yeJCg==; Asynchronous Processing=true";
                }
            }
            else
            {
                if (pTamResource == "BroadcastConnectionString")
                {
                    return
                        @"Data Source=DEVFSQL2014\MAESTRO2_DEV;Initial Catalog=broadcast_integration_staging;  Persist Security Info=True;user id=tamservice;pwd=KFqUjr+SjgugpL7h7yeJCg==; Asynchronous Processing=true";
                }
                else if (pTamResource == "BroadcastForecastConnectionString")
                {
                    return
                        @"Data Source=DEVFSQL2014\MAESTRO2_DEV;Initial Catalog=broadcast_forecast_integration_staging;  Persist Security Info=True;user id=tamservice;pwd=KFqUjr+SjgugpL7h7yeJCg==; Asynchronous Processing=true";
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
                    result = "Password01";
                    break;
                case "WWTV_FtpHost":
                    result = "localhost";
                    break;
                case "WWTV_FtpOutboundFolder":
                    result = "uploads";
                    break;
                case "WWTV_FtpErrorFolder":
                    result = "Errors";
                    break;
                case "EmailHost": 
                    result = "smtp.office365.com";
                    break;
                case "EmailFrom":
                    result = "broadcast@crossmw.com";
                    break;
                case "EmailUsername":
                    result = "traffic@crossmw.com";
                    break;
                case "EmailPassword":
                    result = "JMnxeJ1eBhqAsFnqv/lr4w==";
                    break;
                case "EmailWhiteList":
                    result = "mhohenshilt@crossmw.com";
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