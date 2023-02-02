using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;

namespace Services.Broadcast.Repositories.SpotExceptions
{
    public interface ISpotExceptionsRecommendedPlanRepositoryV2 : IDataRepository
    {
        /// <summary>
        /// Gets the recommended plan decision queued count asynchronous.
        /// </summary>
        /// <returns></returns>
        int GetRecommendedPlanDecisionQueuedCountAsync();
    }

    public class SpotExceptionsRecommendedPlanRepositoryV2 : BroadcastRepositoryBase, ISpotExceptionsRecommendedPlanRepositoryV2
    {
        public SpotExceptionsRecommendedPlanRepositoryV2(
            IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationSettingsHelper configurationSettingsHelper)
        : base(pBroadcastContextFactory, pTransactionHelper, configurationSettingsHelper)
        { }

        /// <inheritdoc />
        public int GetRecommendedPlanDecisionQueuedCountAsync()
        {
            return _InReadUncommitedTransaction(context =>
            {
                var recommandedPlansDecisionCount = context.spot_exceptions_recommended_plan_done_decisions
                  .Where(x => x.synced_at == null)
                  .Count();

                return recommandedPlansDecisionCount;
            });
        }
    }
}
