using Services.Broadcast.Cache;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.IntegrationTests.Stubs
{
    public class ShowTypeCacheStub : IShowTypeCache
    {
        public ShowTypeDto GetMaestroShowTypeByMasterShowType(ShowTypeDto masterShowType)
        {
            throw new System.NotImplementedException();
        }

        public LookupDto GetMaestroShowTypeLookupDtoByName(string name)
        {
            return new LookupDto { Id = 1, Display = name };
        }

        public ShowTypeDto GetMaestroShowTypeByName(string name)
        {
            return new ShowTypeDto { Id = 1, Name = name, ShowTypeSource = ProgramSourceEnum.Maestro };
        }

        public ShowTypeDto GetMasterShowTypeByMaestroShowType(ShowTypeDto maestroShowType)
        {
            throw new System.NotImplementedException();
        }

        public ShowTypeDto GetMasterShowTypeByName(string name)
        {
            throw new System.NotImplementedException();
        }

        public LookupDto GetMasterShowTypeLookupDtoByName(string name)
        {
            throw new System.NotImplementedException();
        }
    }
}
