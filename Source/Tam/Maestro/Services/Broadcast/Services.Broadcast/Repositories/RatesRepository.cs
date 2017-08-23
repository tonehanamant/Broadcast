using Common.Services.Extensions;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using System;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.Repositories
{
    public interface IRatesRepository : IDataRepository
    {
        int GetRateFileIdByHash(string hash);
        int CreateRatesFile(RatesFile file, string userName);
        void UpdateRatesFile(RatesFile ratesFile, string userName);
        void DeleteRatesFileById(int ratesFileId);
        void UpdateRatesFileStatus(int fileId, RatesFile.FileStatusEnum status);

    }

    public class RatesRepository: BroadcastRepositoryBase, IRatesRepository
    {

        public RatesRepository(ISMSClient pSmsClient, IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory, 
            ITransactionHelper pTransactionHelper) : base(pSmsClient, pBroadcastContextFactory, pTransactionHelper)
        {
        }

        public int GetRateFileIdByHash(string hash)
        {
            var result = _InReadUncommitedTransaction(
                context => (
                    from x in context.rate_files
                    where x.file_hash == hash
                    && (x.status == (short) RatesFile.FileStatusEnum.Loaded
                        || x.status == (short) RatesFile.FileStatusEnum.Pending)
                    select x.id).FirstOrDefault());
            return result;
        }

        public int CreateRatesFile(RatesFile ratesFile, string userName)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var file = new rate_files();
                    file.identifier = ratesFile.UniqueIdentifier;
                    file.name = ratesFile.FileName;
                    file.status = (byte) ratesFile.FileStatus;
                    file.file_hash = ratesFile.Hash;
                    file.created_by = userName;
                    file.created_date = DateTime.Now;
                    file.rate_source = (byte) ratesFile.RateSource;
                    file.sweep_book_id = ratesFile.RatingBook;
                    file.play_back_type = (byte)ratesFile.PlaybackType;
                    context.rate_files.Add(file);
                    
                    context.SaveChanges();
                    return file.id;
                });

        }

        public void UpdateRatesFile(RatesFile ratesFile, string userName)
        {
            rate_files file = null;

            _InReadUncommitedTransaction(
                context =>
                {

                    file =
                        context.rate_files.Where(rf => rf.id == ratesFile.Id)
                            .Single(string.Format("Could not find existing rates file with id={0}", ratesFile.Id));

                    file.status = (byte)ratesFile.FileStatus;
                    file.identifier = ratesFile.UniqueIdentifier;
                    
                    context.SaveChanges();

                });
        }

        public void DeleteRatesFileById(int ratesFileId)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var file = context.rate_files.Find(ratesFileId);
                    if(file != null) context.rate_files.Remove(file);
                    context.SaveChanges();
                });
        }

        public void UpdateRatesFileStatus(int fileId, RatesFile.FileStatusEnum status)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var ratesFile = context.rate_files.Where(f => f.id == fileId).Single();
                    ratesFile.status = (byte) status;
                    context.SaveChanges();

                });
        }

        
    }    
}
