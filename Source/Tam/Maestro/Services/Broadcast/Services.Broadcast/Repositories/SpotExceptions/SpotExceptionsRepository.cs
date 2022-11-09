using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities.SpotExceptions;
using System;
using System.Linq;
using System.Threading.Tasks;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;

namespace Services.Broadcast.Repositories.SpotExceptions
{
    public interface ISpotExceptionsRepository : IDataRepository
    {
        bool ClearSpotExceptionAllData();

        Task<bool> SyncOutOfSpecDecisionsAsync(TriggerDecisionSyncRequestDto triggerDecisionSyncRequest, DateTime dateTime);

        Task<bool> SyncRecommendedPlanDecisionsAsync(TriggerDecisionSyncRequestDto triggerDecisionSyncRequest, DateTime dateTime);

        Task<int> GetOutOfSpecDecisionQueuedCountAsync();

        Task<int> GetRecommendedPlanDecisionQueuedCountAsync();
    }

    /// <inheritdoc />
    public class SpotExceptionsRepository : BroadcastRepositoryBase, ISpotExceptionsRepository
    {
        public SpotExceptionsRepository(
            IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationSettingsHelper configurationSettingsHelper)
        : base(pBroadcastContextFactory, pTransactionHelper, configurationSettingsHelper)
        { }

        public bool ClearSpotExceptionAllData()
        {
            return _InReadUncommitedTransaction(context =>
            {
                context.Database.ExecuteSqlCommand("DELETE FROM spot_exceptions_recommended_plan_details");
                context.Database.ExecuteSqlCommand("DELETE FROM spot_exceptions_recommended_plans");
                context.Database.ExecuteSqlCommand("DELETE FROM spot_exceptions_recommended_plan_done_decisions");
                context.Database.ExecuteSqlCommand("DELETE FROM spot_exceptions_recommended_plan_details_done");
                context.Database.ExecuteSqlCommand("DELETE FROM spot_exceptions_recommended_plans_done");
                context.Database.ExecuteSqlCommand("DELETE FROM spot_exceptions_out_of_specs");
                context.Database.ExecuteSqlCommand("DELETE FROM spot_exceptions_out_of_spec_done_decisions");
                context.Database.ExecuteSqlCommand("DELETE FROM spot_exceptions_out_of_specs_done");
                context.Database.ExecuteSqlCommand("DELETE FROM spot_exceptions_unposted_no_plan");
                context.Database.ExecuteSqlCommand("DELETE FROM spot_exceptions_unposted_no_reel_roster");
                return true;
            });
        }

        /// <inheritdoc />
        public async Task<bool> SyncOutOfSpecDecisionsAsync(TriggerDecisionSyncRequestDto triggerDecisionSyncRequest, DateTime dateTime)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var spotExceptionsDecisionsEntities = context.spot_exceptions_out_of_spec_done_decisions
                    .Where(spotExceptionsDecisionDb => spotExceptionsDecisionDb.synced_at == null).ToList();

                if (spotExceptionsDecisionsEntities?.Any() ?? false)
                {
                    spotExceptionsDecisionsEntities.ForEach(x => { x.synced_by = triggerDecisionSyncRequest.UserName; x.synced_at = dateTime; });
                }

                bool isSpotExceptionsOutOfSpecDecisionSynced = false;
                int recordCount = context.SaveChanges();
                isSpotExceptionsOutOfSpecDecisionSynced = recordCount > 0;
                return isSpotExceptionsOutOfSpecDecisionSynced;
            });
        }

        /// <inheritdoc />
        public async Task<bool> SyncRecommendedPlanDecisionsAsync(TriggerDecisionSyncRequestDto triggerDecisionSyncRequest, DateTime dateTime)
        {
            return _InReadUncommitedTransaction(context =>
            {

                var spotExceptionsRecommandedDecisionsEntities = context.spot_exceptions_recommended_plan_done_decisions
                    .Where(spotExceptionsDecisionDb => spotExceptionsDecisionDb.synced_at == null).ToList();

                if (spotExceptionsRecommandedDecisionsEntities?.Any() ?? false)
                {
                    spotExceptionsRecommandedDecisionsEntities.ForEach(x => { x.synced_by = triggerDecisionSyncRequest.UserName; x.synced_at = dateTime; });
                }

                bool isSpotExceptionsRecommandedPlanDecisionSynced = false;
                int recordCount = context.SaveChanges();
                isSpotExceptionsRecommandedPlanDecisionSynced = recordCount > 0;
                return isSpotExceptionsRecommandedPlanDecisionSynced;
            });
        }

        /// <inheritdoc />
        public async Task<int> GetOutOfSpecDecisionQueuedCountAsync()
        {
            return _InReadUncommitedTransaction(context =>
            {
                var OutOfSpecDecisionCount = context.spot_exceptions_out_of_spec_done_decisions
                  .Where(x => x.synced_at == null)
                  .Count();

                return OutOfSpecDecisionCount;
            });
        }

        /// <inheritdoc />
        public async Task<int> GetRecommendedPlanDecisionQueuedCountAsync()
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
