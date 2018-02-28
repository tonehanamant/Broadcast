using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.Repositories
{

    public interface IAffidavitPreprocessingRepository : IDataRepository
    {
        /// <summary>
        /// Persists a List of OutboundAffidavitFileValidationResultDto objects
        /// </summary>
        /// <param name="model">List of OutboundAffidavitFileValidationResultDto objects to be saved</param>
        void SaveValidationObject(List<OutboundAffidavitFileValidationResultDto> model);
    }

    public class AffidavitPreprocessingRepository : BroadcastRepositoryBase, IAffidavitPreprocessingRepository
    {
        public AffidavitPreprocessingRepository(ISMSClient pSmsClient, IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory, ITransactionHelper pTransactionHelper)
            : base(pSmsClient, pBroadcastContextFactory, pTransactionHelper)
        {
        }

        /// <summary>
        /// Persists a List of OutboundAffidavitFileValidationResultDto objects
        /// </summary>
        /// <param name="model">List of OutboundAffidavitFileValidationResultDto objects to be saved</param>
        public void SaveValidationObject(List<OutboundAffidavitFileValidationResultDto> model)
        {
            _InReadUncommitedTransaction(context =>
            {
                context.affidavit_outbound_files.AddRange(model.Select(item =>
                    new affidavit_outbound_files()
                    {
                        created_date = item.CreatedDate,
                        file_hash = item.FileHash,
                        file_name = item.FileName,
                        source_id = item.SourceId,
                        status = item.Status,
                        created_by = item.CreatedBy,
                        affidavit_outbound_file_problems = item.ErrorMessages.Select(y => new affidavit_outbound_file_problems()
                        {
                            problem_description = y
                        }).ToList()
                    }).ToList());
                context.SaveChanges();
            });
        }
    }
}
