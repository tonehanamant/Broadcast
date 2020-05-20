using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.Entities.DTO;
using Services.Broadcast.Repositories;
using System.Collections.Generic;
using Tam.Maestro.Services.Cable.SystemComponentParameters;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.ApplicationServices
{
    public interface IEnvironmentService : IApplicationService
    {
        Dictionary<string, string> GetDbInfo();
        EnvironmentDto GetEnvironmentInfo();
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

        public EnvironmentDto GetEnvironmentInfo()
        {
            var appSettings = new AppSettings();
            var environment = appSettings.Environment.ToString();

            return new EnvironmentDto
            {
                Environment = environment,
                DisplayCampaignLink = BroadcastServiceSystemParameter.DisplayCampaignLink,
                DisplayBuyingLink = BroadcastServiceSystemParameter.DisplayBuyingLink,
                AllowMultipleCreativeLengths = BroadcastServiceSystemParameter.AllowMultipleCreativeLengths
            };
        }
    }
}
