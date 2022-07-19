using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using System;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;

namespace Services.Broadcast.Repositories
{
    public interface ISharedFolderFilesRepository : IDataRepository
    {
        Guid SaveFile(SharedFolderFile sharedFolderFile);

        SharedFolderFile GetFileById(Guid fileId);

        void RemoveFile(Guid fileId);
    }

    public class SharedFolderFilesRepository : BroadcastRepositoryBase, ISharedFolderFilesRepository
    {
        public SharedFolderFilesRepository(
            IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationSettingsHelper configurationSettingsHelper)
            : base(pBroadcastContextFactory, pTransactionHelper, configurationSettingsHelper)
        {
        }

        public SharedFolderFile GetFileById(Guid fileId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var file = context.shared_folder_files.Find(fileId);

                return file == null ? null : new SharedFolderFile
                {
                    Id = file.id,
                    FolderPath = file.folder_path,
                    FileName = file.file_name,
                    FileExtension = file.file_extension,
                    FileMediaType = file.file_media_type,
                    FileUsage = (SharedFolderFileUsage)file.file_usage,
                    CreatedDate = file.created_date,
                    CreatedBy = file.created_by,
                    AttachmentId = file.attachment_id
                };
            });
        }

        public void RemoveFile(Guid fileId)
        {
            _InReadUncommitedTransaction(context =>
            {
                var file = context.shared_folder_files.Find(fileId);

                if (file != null)
                {
                    context.shared_folder_files.Remove(file);
                    context.SaveChanges();
                }
            });
        }

        public Guid SaveFile(SharedFolderFile sharedFolderFile)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var fileToSave = new shared_folder_files
                {
                    id = Guid.NewGuid(),
                    folder_path = sharedFolderFile.FolderPath,
                    file_name = sharedFolderFile.FileName,
                    file_extension = sharedFolderFile.FileExtension,
                    file_media_type = sharedFolderFile.FileMediaType,
                    file_usage = (int)sharedFolderFile.FileUsage,
                    created_date = sharedFolderFile.CreatedDate,
                    created_by = sharedFolderFile.CreatedBy,
                    attachment_id = sharedFolderFile.AttachmentId
                };

                context.shared_folder_files.Add(fileToSave);
                context.SaveChanges();

                return fileToSave.id;
            });
        }
    }
}
