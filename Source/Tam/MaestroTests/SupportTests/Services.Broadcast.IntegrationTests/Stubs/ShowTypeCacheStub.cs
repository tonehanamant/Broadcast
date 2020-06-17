using Services.Broadcast.Cache;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.IntegrationTests.Stubs
{
    public class ShowTypeCacheStub : IShowTypeCache
    {
        public LookupDto GetShowTypeByName(string name)
        {
            return new LookupDto { Id = 1, Display = name };
        }
    }
}
