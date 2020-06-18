using Common.Services.Repositories;
using ConfigurationService.Client;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Helpers;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;

namespace Services.Broadcast.Repositories
{
    public interface IDataMaintenanceRepository : IDataRepository
    {
        void HtmlDecodeProgramNames();
    }

    public class DataMaintenanceRepository : BroadcastRepositoryBase, IDataMaintenanceRepository
    {
        public DataMaintenanceRepository(
            IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, 
            IConfigurationWebApiClient pConfigurationWebApiClient)
            : base(pBroadcastContextFactory, pTransactionHelper, pConfigurationWebApiClient) { }

        public void HtmlDecodeProgramNames()
        {
            _InReadUncommitedTransaction(context =>
            {
                foreach (var programMapping in context.program_name_mappings.ToList())
                {
                    programMapping.inventory_program_name = WebUtilityHelper.HtmlDecodeProgramName(programMapping.inventory_program_name);
                    programMapping.official_program_name = WebUtilityHelper.HtmlDecodeProgramName(programMapping.official_program_name);
                }

                foreach (var manifestDaypart in context.station_inventory_manifest_dayparts.ToList())
                {
                    manifestDaypart.program_name = WebUtilityHelper.HtmlDecodeProgramName(manifestDaypart.program_name);
                }

                context.SaveChanges();
            });
        }
    }
}
