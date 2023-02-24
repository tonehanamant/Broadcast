using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities.SpotExceptions;
using Services.Broadcast.Entities.SpotExceptions.DecisionSync;
using Services.Broadcast.Entities.DTO.SpotExceptionsApi;
using System.Threading.Tasks;
using System;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using System.Linq;

namespace Services.Broadcast.Repositories.SpotExceptions
{
    public interface ISpotExceptionsRepositoryV2 : IDataRepository
    {
        /// <summary>
        /// Synchronizes the out of spec decisions asynchronous.
        /// </summary>
        /// <param name="triggerDecisionSyncRequest">The trigger decision synchronize request.</param>
        /// <param name="dateTime">The date time.</param>
        /// <returns></returns>
        Task<bool> SyncOutOfSpecDecisionsAsync(TriggerDecisionSyncRequestDto triggerDecisionSyncRequest, DateTime dateTime);

        /// <summary>
        /// Synchronizes the recommended plan decisions asynchronous.
        /// </summary>
        /// <param name="triggerDecisionSyncRequest">The trigger decision synchronize request.</param>
        /// <param name="dateTime">The date time.</param>
        /// <returns></returns>
        Task<bool> SyncRecommendedPlanDecisionsAsync(TriggerDecisionSyncRequestDto triggerDecisionSyncRequest, DateTime dateTime);

        /// <summary>
        /// Gets the running synchronize run identifier.
        /// </summary>
        /// <returns></returns>
        Task<int> GetRunningSyncRunId();

        /// <summary>
        /// Saves the running synchronize job.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="date">The date.</param>
        void SaveRunningSyncJob(ResultsSyncRequest request, DateTime date);

        /// <summary>
        /// Sets the current job to complete.
        /// </summary>
        /// <param name="request">The request.</param>
        void SetCurrentJobToComplete(GetSyncStateResponseDto request);

        /// <summary>
        /// Resets the spot exception results indicator.
        /// </summary>
        /// <returns></returns>
        bool ResetSpotExceptionResultsIndicator();
    }

    public class SpotExceptionsRepositoryV2 : BroadcastRepositoryBase, ISpotExceptionsRepositoryV2
    {
        public SpotExceptionsRepositoryV2(
            IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationSettingsHelper configurationSettingsHelper)
        : base(pBroadcastContextFactory, pTransactionHelper, configurationSettingsHelper)
        { }

        /// <inheritdoc />
        public Task<bool> SyncOutOfSpecDecisionsAsync(TriggerDecisionSyncRequestDto triggerDecisionSyncRequest, DateTime dateTime)
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

                return Task.FromResult(isSpotExceptionsOutOfSpecDecisionSynced);
            });
        }

        /// <inheritdoc />
        public async Task<bool> SyncRecommendedPlanDecisionsAsync(TriggerDecisionSyncRequestDto triggerDecisionSyncRequest, DateTime dateTime)
        {
            return await _InReadUncommitedTransaction(context =>
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

                return Task.FromResult(isSpotExceptionsRecommandedPlanDecisionSynced);
            });
        }

        /// <inheritdoc />
        public async Task<int> GetRunningSyncRunId()
        {
            return await _InReadUncommitedTransaction(context =>
            {
                var runningSyncRunId = context.spot_exceptions_results_jobs.Where(x => x.completed_at == null).Select(x => x.databricks_run_id).FirstOrDefault();

                return Task.FromResult(runningSyncRunId);
            });
        }

        /// <inheritdoc />
        public void SaveRunningSyncJob(ResultsSyncRequest request, DateTime date)
        {
            _InReadUncommitedTransaction(context =>
            {
                var existingJob = context.spot_exceptions_results_jobs.First();

                if (existingJob != null)
                {
                    existingJob.databricks_job_id = -1;
                    existingJob.databricks_run_id = -1;
                    existingJob.queued_at = date;
                    existingJob.queued_by = request.RequestedBy;
                }
                else
                {
                    var entity = new spot_exceptions_results_jobs
                    {
                        databricks_job_id = -1,
                        databricks_run_id = -1,
                        queued_at = date,
                        queued_by = request.RequestedBy,
                        completed_at = null,
                        result = null
                    };

                    context.spot_exceptions_results_jobs.Add(entity);
                }
               
                context.SaveChanges();
            });
        }

        /// <inheritdoc />
        public void SetCurrentJobToComplete(GetSyncStateResponseDto request)
        {
            _InReadUncommitedTransaction(context =>
            {
                var existingJob = context.spot_exceptions_results_jobs.Where(x => x.databricks_run_id == request.Result.RunId).First();

                existingJob.completed_at = DateTime.Now;
                existingJob.result = request.ToString();

                context.SaveChanges();
            });
        }

        /// <inheritdoc />
        public bool ResetSpotExceptionResultsIndicator()
        {
            return _InReadUncommitedTransaction(context =>
            {
                var existingJob = context.spot_exceptions_results_jobs.FirstOrDefault();

                if (existingJob == null)
                {
                    return true;
                }
                existingJob.completed_at = DateTime.Now;
                existingJob.result = "Completed manually by the results indicator reset process";

                context.SaveChanges();
                return true;
            });
        }
    }
}
