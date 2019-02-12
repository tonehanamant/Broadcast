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
    public interface IInventoryFileRepository : IDataRepository
    {
        int GetInventoryFileIdByHash(string hash);
        int CreateInventoryFile(InventoryFile file, string userName);
        void UpdateInventoryFile(InventoryFile inventoryFile, string userName);
        void DeleteInventoryFileById(int inventoryFileId);
        void UpdateInventoryFileStatus(int fileId, InventoryFile.FileStatusEnum status);
        InventoryFile GetInventoryFileById(int fileId);
    }

    public class InventoryFileRepository: BroadcastRepositoryBase, IInventoryFileRepository
    {

        public InventoryFileRepository(ISMSClient pSmsClient, IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory, 
            ITransactionHelper pTransactionHelper) : base(pSmsClient, pBroadcastContextFactory, pTransactionHelper)
        {
        }

        public int GetInventoryFileIdByHash(string hash)
        {
            var result = _InReadUncommitedTransaction(
                context => (
                    from x in context.inventory_files
                    where x.file_hash == hash
                    && (x.status == (short) InventoryFile.FileStatusEnum.Loaded
                        || x.status == (short) InventoryFile.FileStatusEnum.Pending)
                    select x.id).FirstOrDefault());
            return result;
        }

        public int CreateInventoryFile(InventoryFile inventoryFile, string userName)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var file = new inventory_files
                    {
                        identifier = inventoryFile.UniqueIdentifier,
                        name = inventoryFile.FileName,
                        status = (byte) inventoryFile.FileStatus,
                        file_hash = inventoryFile.Hash,
                        created_by = userName,
                        created_date = DateTime.Now,
                        inventory_source_id = inventoryFile.InventorySource.Id,
                        sweep_book_id = inventoryFile.RatingBook,
                        play_back_type = (byte?) inventoryFile.PlaybackType
                    };

                    context.inventory_files.Add(file);
                    context.SaveChanges();
                    
                    return file.id;
                });

        }

        public void UpdateInventoryFile(InventoryFile inventoryFile, string userName)
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
                    if(file != null) context.inventory_files.Remove(file);
                    context.SaveChanges();
                });
        }

        public void UpdateInventoryFileStatus(int fileId, InventoryFile.FileStatusEnum status)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var ratesFile = context.inventory_files.Where(f => f.id == fileId).Single();
                    ratesFile.status = (byte) status;
                    context.SaveChanges();

                });
        }

        public InventoryFile GetInventoryFileById(int fileId)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var file = context.inventory_files.Single(x => x.id == fileId, $"Could not find existing file with id={fileId}");

                    return new InventoryFile
                    {
                        Id = file.id,
                        FileName = file.name,
                        FileStatus = (InventoryFile.FileStatusEnum)file.status,
                        Hash = file.file_hash,
                        UniqueIdentifier = file.identifier,
                        InventorySource = new InventorySource
                        {
                            Id = file.inventory_sources.id,
                            InventoryType = (InventoryType)file.inventory_sources.inventory_source_type,
                            IsActive = file.inventory_sources.is_active,
                            Name = file.inventory_sources.name
                        },
                        RatingBook = file.sweep_book_id,
                        PlaybackType = (Entities.Enums.ProposalEnums.ProposalPlaybackType?)file.play_back_type
                    };
                });
        }
    }    
}
