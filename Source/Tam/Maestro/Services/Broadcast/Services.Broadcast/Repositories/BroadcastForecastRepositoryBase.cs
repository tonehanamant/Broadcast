using Common.Services.Repositories;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Data.EntityFrameworkMapping.BroadcastForecast;
using Tam.Maestro.Services.Clients;
using Tam.Maestro.Services.ContractInterfaces;

namespace Services.Broadcast.Repositories
{
    public class BroadcastForecastRepositoryBase : CoreRepositoryBase<QueryHintBroadcastForecastContext>
    {
        public BroadcastForecastRepositoryBase(ISMSClient pSmsClient, IContextFactory<QueryHintBroadcastForecastContext> pBroadcastContextFactory, ITransactionHelper pTransactionHelper)
            : base(pSmsClient, pBroadcastContextFactory, pTransactionHelper, TAMResource.BroadcastForecastConnectionString.ToString())
        {
        }
    }
}
