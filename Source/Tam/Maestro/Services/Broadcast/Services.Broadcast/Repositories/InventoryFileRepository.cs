﻿using Common.Services.Extensions;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;

namespace Services.Broadcast.Repositories
{
    public interface IInventoryFileRepository : IDataRepository
    {
        int GetInventoryFileIdByHash(string hash);
        int CreateInventoryFile(InventoryFileBase file, string userName, DateTime nowDate);
        void UpdateInventoryFile(InventoryFile inventoryFile);

        void SaveUploadedFileId(int fileId, Guid uploadedSharedFolderFileId);

        void SaveErrorFileId(int fileId, Guid errorSharedFolderFileId);

        List<Guid> GetErrorFileSharedFolderFileIds(List<int> fileIds);

        void DeleteInventoryFileById(int inventoryFileId);
        void UpdateInventoryFileStatus(int fileId, FileStatusEnum status);
        InventoryFile GetInventoryFileById(int fileId);

        int GetLatestInventoryFileIdByName(string fileName);

        string GetDbInfo();
    }

    public class InventoryFileRepository : BroadcastRepositoryBase, IInventoryFileRepository
    {

        public InventoryFileRepository(IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationSettingsHelper configurationSettingsHelper)
            : base(pBroadcastContextFactory, pTransactionHelper, configurationSettingsHelper) { }

        public InventoryFile GetInventoryFileById(int fileId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var file = context.inventory_files
                    .Single(x => x.id == fileId, $"Could not find existing file with id={fileId}");

                var result = new InventoryFile
                {
                    Id = file.id,
                    FileName = file.name,
                    FileStatus = (FileStatusEnum)file.status,
                    Hash = file.file_hash,
                    UniqueIdentifier = file.identifier,
                    CreatedBy = file.created_by,
                    CreatedDate = file.created_date,
                    EffectiveDate = file.effective_date,
                    EndDate = file.end_date,
                    InventorySource = new InventorySource
                    {
                        Id = file.inventory_sources.id,
                        InventoryType = (InventorySourceTypeEnum)file.inventory_sources.inventory_source_type,
                        IsActive = file.inventory_sources.is_active,
                        Name = file.inventory_sources.name
                    },
                    UploadedFileSharedFolderFileId = file.shared_folder_files_id,
                    ErrorFileSharedFolderFileId = file.error_file_shared_folder_files_id
                };

                return result;
            });
        }

        public int GetLatestInventoryFileIdByName(string fileName)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var latestId = context.inventory_files
                    .Where(s => s.name.Equals(fileName, StringComparison.OrdinalIgnoreCase) &&
                                s.status == (short)FileStatusEnum.Loaded)
                    .Select(s => s.id)
                    .OrderByDescending(s => s)
                    .FirstOrDefault();

                return latestId;
            });
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
                    file = context.inventory_files
                        .Where(rf => rf.id == inventoryFile.Id)
                        .Single($"Could not find existing rates file with id={inventoryFile.Id}");

                    file.status = (byte)inventoryFile.FileStatus;
                    file.identifier = inventoryFile.UniqueIdentifier;
                    file.rows_processed = inventoryFile.RowsProcessed;
                    file.effective_date = inventoryFile.EffectiveDate;
                    file.end_date = inventoryFile.EndDate;

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

        public void SaveUploadedFileId(int fileId, Guid uploadedSharedFolderFileId)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var inventoryFile = context.inventory_files.Where(f => f.id == fileId).Single();
                    inventoryFile.shared_folder_files_id = uploadedSharedFolderFileId;
                    context.SaveChanges();
                });
        }

        public void SaveErrorFileId(int fileId, Guid errorSharedFolderFileId)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var inventoryFile = context.inventory_files.Where(f => f.id == fileId).Single();
                    inventoryFile.error_file_shared_folder_files_id = errorSharedFolderFileId;
                    context.SaveChanges();
                });
        }

        public List<Guid> GetErrorFileSharedFolderFileIds(List<int> fileIds)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var result = context.inventory_files
                    .Where(f => fileIds.Contains(f.id) && f.error_file_shared_folder_files_id.HasValue)
                    .Select(f => f.error_file_shared_folder_files_id.Value)
                    .ToList();

                return result;
            });
        }
    }
}
