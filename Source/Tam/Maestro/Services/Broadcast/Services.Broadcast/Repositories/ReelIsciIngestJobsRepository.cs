using Common.Services.Extensions;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities.Isci;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;

namespace Services.Broadcast.Repositories
{
    public interface IReelIsciIngestJobsRepository : IDataRepository
    {
        int AddReelIsciIngestJob(ReelIsciIngestJobDto reelIsciIngestJobObj);
        void UpdateReelIsciIngestJob(ReelIsciIngestJobDto reelIsciIngestJobObj);
    }

    public class ReelIsciIngestJobsRepository : BroadcastRepositoryBase, IReelIsciIngestJobsRepository
    {
        public ReelIsciIngestJobsRepository(IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationSettingsHelper configurationSettingsHelper)
            : base(pBroadcastContextFactory, pTransactionHelper, configurationSettingsHelper) { }

        public int AddReelIsciIngestJob(ReelIsciIngestJobDto reelIsciIngestJobObj)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var reelIsciIngestJobDb = new reel_isci_ingest_jobs
                {
                    status = (int)reelIsciIngestJobObj.Status,
                    queued_at = reelIsciIngestJobObj.QueuedAt,
                    queued_by = reelIsciIngestJobObj.QueuedBy
                };

                context.reel_isci_ingest_jobs.Add(reelIsciIngestJobDb);

                context.SaveChanges();

                return reelIsciIngestJobDb.id;
            });
        }
        public void UpdateReelIsciIngestJob(ReelIsciIngestJobDto reelIsciIngestJobObj)
        {
            _InReadUncommitedTransaction(context =>
                {
                    var job = context.reel_isci_ingest_jobs.Single(x => x.id == reelIsciIngestJobObj.Id);

                    job.status = (int)reelIsciIngestJobObj.Status;
                    job.completed_at = reelIsciIngestJobObj.CompletedAt;
                    job.error_message = reelIsciIngestJobObj.ErrorMessage;

                    context.SaveChanges();
                }
            );
        }
    }
}