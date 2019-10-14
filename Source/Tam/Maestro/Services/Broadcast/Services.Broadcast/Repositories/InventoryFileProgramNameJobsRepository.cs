using Common.Services.Repositories;
using ConfigurationService.Client;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;

namespace Services.Broadcast.Repositories
{
    public interface IInventoryFileProgramNameJobsRepository : IDataRepository
    {
        int AddJob(InventoryFileProgramNameJob job);
    }

    public class InventoryFileProgramNameJobsRepository : BroadcastRepositoryBase, IInventoryFileProgramNameJobsRepository
    {
        public InventoryFileProgramNameJobsRepository(IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationWebApiClient pConfigurationWebApiClient)
            : base(pBroadcastContextFactory, pTransactionHelper, pConfigurationWebApiClient) { }

        public int AddJob(InventoryFileProgramNameJob job)
        {
            return _InReadUncommitedTransaction(context =>
                {
                    var fileJob = new inventory_file_program_names_jobs
                    {
                        inventory_file_id = job.InventoryFileId,
                        status = (int)job.Status,
                        queued_at = job.QueuedAt,
                        queued_by = job.QueuedBy,
                        completed_at = null
                    };

                    context.inventory_file_program_names_jobs.Add(fileJob);
                    context.SaveChanges();

                    return fileJob.id;
                }
            );
        }
    }
}