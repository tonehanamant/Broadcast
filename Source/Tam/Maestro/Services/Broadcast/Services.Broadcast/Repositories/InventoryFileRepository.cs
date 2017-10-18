﻿using Common.Services.Extensions;
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
                    var file = new inventory_files();
                    file.identifier = inventoryFile.UniqueIdentifier;
                    file.name = inventoryFile.FileName;
                    file.status = (byte) inventoryFile.FileStatus;
                    file.file_hash = inventoryFile.Hash;
                    file.created_by = userName;
                    file.created_date = DateTime.Now;
                    file.inventory_source = (byte)inventoryFile.Source;
                    file.sweep_book_id = inventoryFile.RatingBook;
                    file.play_back_type = (byte)inventoryFile.PlaybackType;
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

        
    }    
}
