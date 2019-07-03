using Common.Services.Repositories;
using ConfigurationService.Client;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Data.EntityFrameworkMapping.BroadcastForecast;
using Tam.Maestro.Services.Clients;
using Tam.Maestro.Services.ContractInterfaces;

namespace Services.Broadcast.Repositories
{
    public class BroadcastForecastRepositoryBase : RepositoryBase<QueryHintBroadcastForecastContext>
    {
        public BroadcastForecastRepositoryBase(ISMSClient pSmsClient, IContextFactory<QueryHintBroadcastForecastContext> pBroadcastContextFactory, ITransactionHelper pTransactionHelper, IConfigurationWebApiClient configurationWebApiClient)
            : base(pSmsClient, pBroadcastContextFactory, pTransactionHelper, configurationWebApiClient, TAMResource.BroadcastForecastConnectionString.ToString())
        {
        }
    }
}
