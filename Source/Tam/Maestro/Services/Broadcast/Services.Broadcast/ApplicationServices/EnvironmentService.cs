using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.ApplicationServices
{
    public interface IEnvironmentService : IApplicationService
    {
        Dictionary<string, string> GetDbInfo();
    }
    public class EnvironmentService: IEnvironmentService
    {
        private readonly IRatingsRepository _RatingsRepo;
        private readonly IInventoryFileRepository _InventoryFileRepo;
        public EnvironmentService(IDataRepositoryFactory broadcastDataRepositoryFactory)
        {
            _RatingsRepo = broadcastDataRepositoryFactory.GetDataRepository<IRatingsRepository>();
            _InventoryFileRepo = broadcastDataRepositoryFactory.GetDataRepository<IInventoryFileRepository>();
        }

        public Dictionary<string, string> GetDbInfo()
        {
            var result = new Dictionary<string, string>();
            result.Add("broadcast", _InventoryFileRepo.GetDbInfo());
            result.Add("broadcastforecast", _RatingsRepo.GetDbInfo());
            return result;
        }

    }
}
