using System;
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
            IConfigurationWebApiClient pConfigurationWebApiClient, IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper)
            : base(pBroadcastContextFactory, pTransactionHelper, pConfigurationWebApiClient, featureToggleHelper, configurationSettingsHelper) { }

        public void HtmlDecodeProgramNames()
        {
            try
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
            catch (Exception e)
            {
                _LogError("Exception caught decoding.", e);
                if (e.InnerException?.InnerException?.Message?.Contains("Cannot insert duplicate key row in object") == true)
                {
                    var duplicateKeyException = e.InnerException.InnerException;
                    throw new InvalidOperationException($"Duplicate key exception caught attempting to decode program names. : {duplicateKeyException.Message}", duplicateKeyException);
                }
                
                throw;
            }
        }
    }
}
