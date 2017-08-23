using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;


namespace Services.Broadcast.Repositories
{
    public interface ISpotLengthMultiplierRepository : IDataRepository
    {
        Dictionary<int, float> GetSpotLengthIdsAndCostMultipliers();
    }

    public class SpotLengthMultiplierRepository : BroadcastRepositoryBase, ISpotLengthMultiplierRepository
    {

        public SpotLengthMultiplierRepository(ISMSClient pSmsClient,
            IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory
            , ITransactionHelper pTransactionHelper)
            : base(pSmsClient, pBroadcastContextFactory, pTransactionHelper)
        {
        }

        public Dictionary<int, float> GetSpotLengthIdsAndCostMultipliers()
        {
            return _InReadUncommitedTransaction(
                context => 
                {
                    return context.spot_length_cost_multipliers.ToDictionary(x => x.spot_length_id, y => (float) y.cost_multiplier);
                });
        }
    }

}
