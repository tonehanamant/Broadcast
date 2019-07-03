using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using System.Collections.Generic;
using System.Linq;
using ConfigurationService.Client;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.Repositories
{
    public interface IProgramRepository : IDataRepository
    {
        List<string> GetUniqueProgramNamesByManifests(List<int> manifestIds);
    }

    public class ProgramRepository : BroadcastRepositoryBase, IProgramRepository
    {
        public ProgramRepository(ISMSClient pSmsClient, IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationWebApiClient pConfigurationWebApiClient)
            : base(pSmsClient, pBroadcastContextFactory, pTransactionHelper, pConfigurationWebApiClient) { }

        public List<string> GetUniqueProgramNamesByManifests(List<int> manifestIds)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return context.station_inventory_manifest_dayparts
                        .Where(x => manifestIds.Contains(x.station_inventory_manifest_id) && x.program_name != null && x.program_name != string.Empty)
                        .Select(x => x.program_name)
                        .Distinct()
                        .ToList();
                });
        }
    }
}
