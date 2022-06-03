using Common.Services.Extensions;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities.Enums;
using System;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;

namespace Services.Broadcast.Repositories
{
    public interface ISpotExceptionsIngestJobRepository : IDataRepository
    {
        void SetJobToError(int jobId, string username, DateTime currentDateTime);
    }

    public class SpotExceptionsIngestJobRepository : BroadcastRepositoryBase, ISpotExceptionsIngestJobRepository
    {

        public SpotExceptionsIngestJobRepository(
            IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationSettingsHelper configurationSettingsHelper)
            : base(pBroadcastContextFactory, pTransactionHelper, configurationSettingsHelper) 
        { 
        }

        public void SetJobToError(int jobId, string username, DateTime currentDateTime)
        {
            _InReadUncommitedTransaction(context =>
            {
                var job = context.spot_exceptions_ingest_jobs.Single(j => j.id == jobId, $"Job with id '{jobId}' not found.");
                
                job.status = (int)BackgroundJobProcessingStatus.Failed;
                job.error_message = $"Forced to error status by '{username}'.";
                job.completed_at = currentDateTime;

                context.SaveChanges();
            });
        }
    }
}
