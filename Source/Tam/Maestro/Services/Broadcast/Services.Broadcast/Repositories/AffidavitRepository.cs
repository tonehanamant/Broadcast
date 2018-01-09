using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.Repositories
{
    public interface IAffidavitRepository : IDataRepository
    {
        int SaveAffidavitFile(affidavit_files affidatite_file);
        affidavit_files GetAffidavit(int affidavit_id);
    }

    public class AffidavitRepository: BroadcastRepositoryBase, IAffidavitRepository
    {

        public AffidavitRepository(ISMSClient pSmsClient, IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper) : base(pSmsClient, pBroadcastContextFactory, pTransactionHelper)
        {
        }

        public int SaveAffidavitFile(affidavit_files affidatite_file)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    context.affidavit_files.Add(affidatite_file);
                    context.SaveChanges();
                });
            return affidatite_file.id;
        }

        public affidavit_files GetAffidavit(int affidavit_id)
        {
            affidavit_files affidavit_file = null;
            
            _InReadUncommitedTransaction(
                context =>
                {
                    affidavit_file = context.affidavit_files
                        .Include("affidavit_file_details")
                        .Single(a => a.id == affidavit_id);
                });
            return affidavit_file;

        }
    }    
}
