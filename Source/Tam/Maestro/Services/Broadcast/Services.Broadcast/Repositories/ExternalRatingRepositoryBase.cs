using Common.Services.Repositories;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Data.EntityFrameworkMapping.ExternalRating;
using Tam.Maestro.Services.Clients;
using Tam.Maestro.Services.ContractInterfaces;

namespace Services.Broadcast.Repositories
{
    public class ExternalRatingRepositoryBase : CoreRepositoryBase<QueryHintExternalRatingContext>
    {
        public ExternalRatingRepositoryBase(ISMSClient pSmsClient, IContextFactory<QueryHintExternalRatingContext> pContextFactory, ITransactionHelper pTransactionHelper)
            : base(pSmsClient, pContextFactory, pTransactionHelper, TAMResource.ExternalRatingsConnectionString)
        {
        }
    }
}
