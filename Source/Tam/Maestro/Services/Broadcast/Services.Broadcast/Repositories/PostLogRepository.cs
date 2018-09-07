using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.Repositories
{
    public interface IPostLogRepository : IDataRepository
    {
        void SavePreprocessingValidationResults(List<FileValidationResult> validationResults);
    }
    public class PostLogRepository : BroadcastRepositoryBase, IPostLogRepository
    {
        public PostLogRepository(ISMSClient pSmsClient,
                                IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
                                ITransactionHelper pTransactionHelper)
            : base(pSmsClient, pBroadcastContextFactory, pTransactionHelper)
        {
        }

        public void SavePreprocessingValidationResults(List<FileValidationResult> validationResults)
        {
            _InReadUncommitedTransaction(context =>
            {
                context.postlog_outbound_files.AddRange(validationResults.Select(item =>
                    new postlog_outbound_files()
                    {
                        created_date = item.CreatedDate,
                        file_hash = item.FileHash,
                        file_name = item.FileName,
                        source_id = (int) item.Source,
                        status = (int)item.Status,
                        created_by = item.CreatedBy,
                        postlog_outbound_file_problems = item.ErrorMessages.Select(y =>
                            new postlog_outbound_file_problems()
                            {
                                problem_description = y
                            }).ToList()
                    }).ToList());
                context.SaveChanges();
            });
        }
    }
}
