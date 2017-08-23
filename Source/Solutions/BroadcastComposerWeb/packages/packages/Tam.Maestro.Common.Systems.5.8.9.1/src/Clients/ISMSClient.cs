using System;
using System.Collections.Generic;
using Common.Systems.DataTransferObjects;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.ContractInterfaces;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Tam.Maestro.Services.Clients
{
    public static class SMSHelper
    {
        public static TAMEnvironment FromStringToTAMEnvironment(string pString)
        {
            if (pString == TAMEnvironment.DEV.ToString())
            {
                return TAMEnvironment.DEV;
            }
            if (pString == TAMEnvironment.QA.ToString())
            {
                return TAMEnvironment.QA;
            }
            if (pString == TAMEnvironment.UAT.ToString())
            {
                return TAMEnvironment.UAT;
            }
            if (pString == TAMEnvironment.PROD.ToString())
            {
                return TAMEnvironment.PROD;
            }
            if (pString == TAMEnvironment.LOCAL.ToString())
            {
                return TAMEnvironment.LOCAL;
            }
            throw new TypeLoadException("String provided is not valid in order to get an TAMEnvironment enumeration member");
        }
    }

    public interface ISMSClient
    {
        event GenericEvent<TAMService, string> UriChanged;
        event GenericEvent<TAMResource, string> ResourceChanged;

        TAMEnvironment TamEnvironment { get; }
        ServiceStatus GetStatus();
        string GetUri<T>(TAMService tamService) where T : ITAMService;
        SmsDbConnectionInfo GetSmsDbConnectionInfo(TAMResource pTamResource);
        string GetResource(TAMResource pTamResource);
        string GetSystemComponentParameterValue(string pSystemComponentID, string pSystemParameterID);
        bool ClearSystemComponentParameterCache(string pSystemComponentID, string pSystemParameterID);
        void OnUriChanged(ServiceManagerServiceEventArgs pArgs);
        void OnResourceChanged(ServiceManagerResourceEventArgs pArgs);
        bool ValidateEnvironment();
        TAMResult IsLocked(IBusinessEntity[] pEntities);
        TAMResult LockEntity(IBusinessEntity[] pEntities);
        void ReleaseEntity(IBusinessEntity[] pEntities);
        MaestroImage GetLogoImage(CMWImageEnums logoType);
        LockResponse LockObject(string key, string userId);
        ReleaseLockResponse ReleaseObject(string key, string userId);
        bool IsObjectLocked(string key, string userId);
        List<LookupDto> GetActiveAdvertisers();
        List<LookupDto> FindAdvertisersByIds(List<int> advertiserIds);
        LookupDto FindAdvertiserById(int advertiserId);
    }
}