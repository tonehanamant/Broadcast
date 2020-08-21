using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.Repositories;
using System.Collections.Generic;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.ApplicationServices
{
    public interface IShowTypeService : IApplicationService
    {
        List<LookupDto> GetShowTypes();
    }

    public class ShowTypeService : IShowTypeService
    {
        private readonly IShowTypeRepository _ShowTypeRepository;

        public ShowTypeService(IDataRepositoryFactory broadcastDataRepositoryFactory)
        {
            _ShowTypeRepository = broadcastDataRepositoryFactory.GetDataRepository<IShowTypeRepository>();
        }

        public List<LookupDto> GetShowTypes()
        {
            return _ShowTypeRepository.GetMaestroShowTypesLookupDto();
        }
    }
}
