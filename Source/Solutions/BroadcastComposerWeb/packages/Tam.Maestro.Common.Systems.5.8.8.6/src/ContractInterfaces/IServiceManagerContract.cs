using System.Collections.Generic;
using System.ServiceModel;
using Common.Systems.DataTransferObjects;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.Cable.DatabaseDtos;

//using Tam.Maestro.Services.CoverageUniverse.Service.Contract;

namespace Tam.Maestro.Services.ContractInterfaces
{
    [ServiceContract(CallbackContract = typeof(IServiceManagerCallback))]
    public interface IServiceManagerContract : ITAMService
    {
        [OperationContract]
        TAMResult2<string> GetUri(TAMEnvironment p1, TAMService p2);

        [OperationContract]
        TAMResult2<string> GetResource(TAMEnvironment p1, TAMResource p2);

        [OperationContract]
        TAMResult2<string> GetSystemComponentParameterValue(string pSystemComponentID, string pSystemParameterID);

        [OperationContract]
        TAMResult2<bool> ClearSystemComponentParameterCache(string pSystemComponentID, string pSystemParameterID);

        [OperationContract]
        TAMResult2<bool> ChangeServiceSettings(TAMEnvironment p1, ServiceItem[] lItem);

        [OperationContract]
        TAMResult2<bool> ChangeResourceSettings(TAMEnvironment p1, ResourceItem[] lItem);

        [OperationContract]
        TAMResult2<string> TestConnection(TAMEnvironment pTamEnvironment, TAMResource pTamResource);

        [OperationContract]
        void Subscribe();

        [OperationContract]
        void UnSubscribe();

        [OperationContract]
        TAMResult IsLockedBin(byte[] pEntities);

        [OperationContract]
        TAMResult LockEntityBin(byte[] pEntities);

        [OperationContract]
        TAMResult ReleaseEntityBin(byte[] pEntities);

        [OperationContract]
        TAMResult2<MaestroImage> GetLogoImage(CMWImageEnums logoType);

        [OperationContract]
        TAMResult2<List<LookupDto>> GetActiveAdvertisers();

        [OperationContract]
        TAMResult2<List<LookupDto>> FindAdvertisersByIds(List<int> advertisersIds);

        [OperationContract]
        TAMResult2<LookupDto> FindAdvertiserById(int advertiserId);

        [OperationContract]
        TAMResult2<AuthenticationEmployee> GetEmployee(string sssid, bool forceReset);

        [OperationContract]
        LockResponse LockObject(string key, string userId);

        [OperationContract]
        ReleaseLockResponse ReleaseObject(string key, string userId);

        [OperationContract]
        bool IsObjectLocked(string key, string userId);
    }
}
