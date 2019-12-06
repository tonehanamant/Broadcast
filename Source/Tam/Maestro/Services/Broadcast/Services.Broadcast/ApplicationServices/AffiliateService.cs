using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.Repositories;
using System.Collections.Generic;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.ApplicationServices
{
    public interface IAffiliateService : IApplicationService
    {
        List<LookupDto> GetAllAffiliates();
    }

    public class AffiliateService : IAffiliateService
    {
        private readonly IAffiliateRepository _AffiliateRepository;

        public AffiliateService(IDataRepositoryFactory broadcastDataRepositoryFactory)
        {
            _AffiliateRepository = broadcastDataRepositoryFactory.GetDataRepository<IAffiliateRepository>();
        }

        public List<LookupDto> GetAllAffiliates()
        {
            return _AffiliateRepository.GetAllAffiliates();
        }
    }
}
