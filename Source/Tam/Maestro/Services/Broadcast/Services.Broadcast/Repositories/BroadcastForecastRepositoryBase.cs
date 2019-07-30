using Common.Services.Repositories;
using ConfigurationService.Client;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Data.EntityFrameworkMapping.BroadcastForecast;
using Tam.Maestro.Services.Clients;
using Tam.Maestro.Services.ContractInterfaces;

namespace Services.Broadcast.Repositories
{
    public class BroadcastForecastRepositoryBase : CoreRepositoryBase<QueryHintBroadcastForecastContext>
    {
        public BroadcastForecastRepositoryBase(IContextFactory<QueryHintBroadcastForecastContext> pBroadcastContextFactory, ITransactionHelper pTransactionHelper, IConfigurationWebApiClient configurationWebApiClient)
            : base(configurationWebApiClient, pBroadcastContextFactory, pTransactionHelper, TAMResource.BroadcastForecastConnectionString.ToString())
        {
        }

        public string GetDbInfo()
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return $"{context.Database.Connection.DataSource}|{context.Database.Connection.Database}";
                });
        }
    }
}
