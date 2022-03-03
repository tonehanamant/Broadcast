using Common.Services.Repositories;
using Hangfire;
using Services.Broadcast.Repositories;
using System;
using System.Threading.Tasks;

namespace Services.Broadcast.ApplicationServices
{
    /// <summary>
    /// Trigger aggregation jobs.
    /// </summary>
    public interface ICampaignAggregationJobTrigger
    {
        /// <summary>
        /// Triggers the job.
        /// </summary>
        /// <param name="campaignId">The campaign identifier.</param>
        /// <param name="queuedBy">The queued by.</param>
        string TriggerJob(int campaignId, string queuedBy);
    }

    /// <summary>
    /// Trigger aggregation jobs.
    /// </summary>
    /// <seealso cref="ICampaignAggregationJobTrigger" />
    public class CampaignAggregationJobTrigger : ICampaignAggregationJobTrigger
    {
        private readonly ICampaignSummaryRepository _CampaignSummaryRepository;
        private readonly IBackgroundJobClient _BackgroundJobClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="CampaignAggregationJobTrigger"/> class.
        /// </summary>
        /// <param name="dataRepositoryFactory">The data repository factory.</param>
        /// <param name="backgroundJobClient">The background job client.</param>
        public CampaignAggregationJobTrigger(IDataRepositoryFactory dataRepositoryFactory,
            IBackgroundJobClient backgroundJobClient)
        {
            _CampaignSummaryRepository = dataRepositoryFactory.GetDataRepository<ICampaignSummaryRepository>();
            _BackgroundJobClient = backgroundJobClient;
        }

        /// <inheritdoc />
        public string TriggerJob(int campaignId, string queuedBy)
        {
            var result = TriggerJobAsync(campaignId, queuedBy).Result;
            return result;
        }

        private async Task<string> TriggerJobAsync(int campaignId, string queuedBy)
        {
            _CampaignSummaryRepository.SetSummaryProcessingStatusToInProgress(campaignId, queuedBy, DateTime.Now);
            return await Task.Run(() => _BackgroundJobClient.Enqueue<ICampaignService>(x => x.ProcessCampaignAggregation(campaignId)));
        }
    }
}