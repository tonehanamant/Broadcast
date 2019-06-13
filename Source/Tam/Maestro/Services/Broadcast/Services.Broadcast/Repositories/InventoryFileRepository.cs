using Common.Services.Extensions;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using System;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.Repositories
{
    public interface IInventoryFileRepository : IDataRepository
    {
        int GetInventoryFileIdByHash(string hash);
        int CreateInventoryFile(InventoryFileBase file, string userName, DateTime nowDate);
        void UpdateInventoryFile(InventoryFile inventoryFile);
        void DeleteInventoryFileById(int inventoryFileId);
        void UpdateInventoryFileStatus(int fileId, FileStatusEnum status);
    }

    public class InventoryFileRepository : BroadcastRepositoryBase, IInventoryFileRepository
    {

        public InventoryFileRepository(ISMSClient pSmsClient, IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper) : base(pSmsClient, pBroadcastContextFactory, pTransactionHelper)
        {
        }

        public int GetInventoryFileIdByHash(string hash)
        {
            return _InReadUncommitedTransaction(
                context => (
                    from x in context.inventory_files
                    where x.file_hash == hash
                    && (x.status == (short)FileStatusEnum.Loaded
                        || x.status == (short)FileStatusEnum.Pending)
                    select x.id).FirstOrDefault());
        }

        public int CreateInventoryFile(InventoryFileBase inventoryFile, string userName, DateTime nowDate)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var file = new inventory_files
                    {
                        identifier = inventoryFile.UniqueIdentifier,
                        name = inventoryFile.FileName,
                        status = (byte)inventoryFile.FileStatus,
                        file_hash = inventoryFile.Hash,
                        created_by = userName,
                        created_date = nowDate,
                        inventory_source_id = inventoryFile.InventorySource.Id,
                    };

                    context.inventory_files.Add(file);
                    context.SaveChanges();

                    return file.id;
                });
        }

        public void UpdateInventoryFile(InventoryFile inventoryFile)
        {
            inventory_files file = null;

            _InReadUncommitedTransaction(
                context =>
                {
                    file =
                        context.inventory_files.Where(rf => rf.id == inventoryFile.Id)
                            .Single(string.Format("Could not find existing rates file with id={0}", inventoryFile.Id));

                    file.status = (byte)inventoryFile.FileStatus;
                    file.identifier = inventoryFile.UniqueIdentifier;

                    context.SaveChanges();

                });
        }

        public void DeleteInventoryFileById(int inventoryFileId)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var file = context.inventory_files.Find(inventoryFileId);
                    if (file != null) context.inventory_files.Remove(file);
                    context.SaveChanges();
                });
        }

        public void UpdateInventoryFileStatus(int fileId, FileStatusEnum status)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var ratesFile = context.inventory_files.Where(f => f.id == fileId).Single();
                    ratesFile.status = (byte)status;
                    context.SaveChanges();

                });
        }
    }
}
