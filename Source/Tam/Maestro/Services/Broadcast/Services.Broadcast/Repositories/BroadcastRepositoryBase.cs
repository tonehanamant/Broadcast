using EntityFrameworkMapping.Broadcast;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;
using Tam.Maestro.Services.ContractInterfaces;

namespace Common.Services.Repositories
{
    public class BroadcastRepositoryBase : CoreRepositoryBase<QueryHintBroadcastContext>
    {
        public BroadcastRepositoryBase(ISMSClient pSmsClient, IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory, ITransactionHelper pTransactionHelper)
            : base(pSmsClient, pBroadcastContextFactory, pTransactionHelper, TAMResource.BroadcastConnectionString)
        {
        }
    }
}
