using System;
using System.Collections.Generic;
using System.IO;
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
        public TAMEnvironment TamEnvironment { get; private set; }
        public ServiceStatus GetStatus()
        {
            throw new System.NotImplementedException();
        }

        public string GetUri<T>(TAMService tamService) where T : ITAMService
        {
            throw new System.NotImplementedException();
        }

        public SmsDbConnectionInfo GetSmsDbConnectionInfo(TAMResource pTamResource)
        {
            throw new System.NotImplementedException();
        }

        public string GetResource(TAMResource pTamResource)
        {
            throw new NotImplementedException();//return EnvironmentSettings.Handler.GetResource(pTamResource, TAMEnvironment.LOCAL);
        }

        public string GetSystemComponentParameterValue(string pSystemComponentID, string pSystemParameterID)
        {
            if (pSystemParameterID == "BroadcastMatchingBuffer")
            {
                return "120";
            }
            else if (pSystemParameterID == "DaypartCacheSlidingExpirationSeconds")
            {
                return "1800";
            }
            else if (pSystemParameterID == "UseDayByDayImpressions")
            {
                return "False";
            }

            throw new Exception("Unknown SystemComponentParameter: " + pSystemParameterID);
        }

        public bool ClearSystemComponentParameterCache(string pSystemComponentID, string pSystemParameterID)
        {
            throw new System.NotImplementedException();
        }

        public void OnUriChanged(ServiceManagerServiceEventArgs pArgs)
        {
            throw new System.NotImplementedException();
        }

        public void OnResourceChanged(ServiceManagerResourceEventArgs pArgs)
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