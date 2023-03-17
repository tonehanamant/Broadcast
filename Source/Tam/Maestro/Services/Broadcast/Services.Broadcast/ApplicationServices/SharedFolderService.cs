using Common.Services;
using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Services.Broadcast.ApplicationServices
{
    public interface ISharedFolderService : IApplicationService
    {
        Guid SaveFile(SharedFolderFile file);

        SharedFolderFile GetFile(Guid fileId);

        SharedFolderFile GetFileInfo(Guid fileId);

        void RemoveFile(Guid fileId);

        SharedFolderFile GetAndRemoveFile(Guid fileId);

        Stream CreateZipArchive(List<Guid> fileIdsToArchive);

        void RemoveFileFromFileShare(Guid fileId);
    }

    public class SharedFolderService : BroadcastBaseClass, ISharedFolderService
    {
        private readonly ISharedFolderFilesRepository _SharedFolderFilesRepository;
        private readonly IAttachmentMicroServiceApiClient _AttachmentMicroServiceApiClient;

        public SharedFolderService(
            IDataRepositoryFactory broadcastDataRepositoryFactory, IAttachmentMicroServiceApiClient attachmentMicroServiceApiClient, IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper)
            : base(featureToggleHelper, configurationSettingsHelper)
        {
            _SharedFolderFilesRepository = broadcastDataRepositoryFactory.GetDataRepository<ISharedFolderFilesRepository>();
            _AttachmentMicroServiceApiClient = attachmentMicroServiceApiClient;

        }

        public SharedFolderFile GetAndRemoveFile(Guid fileId)
        {
            var file = GetFile(fileId);

            RemoveFile(fileId);

            return file;
        }

        public SharedFolderFile GetFileInfo(Guid fileId)
        {
            var file = _SharedFolderFilesRepository.GetFileById(fileId);

            if (file == null)
                throw new InvalidOperationException($"There is no file with id: {fileId}");

            return file;
        }

        public SharedFolderFile GetFile(Guid fileId)
        {
            var file = GetFileInfo(fileId);
            file.FileContent = _GetFileContent(file);
            return file;
        }

        public void RemoveFile(Guid fileId)
        {
            var file = GetFileInfo(fileId);

            using (var transaction = TransactionScopeHelper.CreateTransactionScopeWrapper(TimeSpan.FromMinutes(20)))
            {
                _SharedFolderFilesRepository.RemoveFile(fileId);
                _RemoveFileContent(file);
                transaction.Complete();
            }
        }

        public Guid SaveFile(SharedFolderFile file)
        {
            using (var transaction = TransactionScopeHelper.CreateTransactionScopeWrapper(TimeSpan.FromMinutes(20)))
            {
                var registerResult = _AttachmentMicroServiceApiClient.RegisterAttachment(file.FileName, file.CreatedBy, "");
                if (registerResult != null)
                {
                    file.AttachmentId = registerResult.AttachmentId;
                }
                _LogInfo($"Attachment is registered with AttachementId = '{file.AttachmentId}'");

                file.Id = _SharedFolderFilesRepository.SaveFile(file);
                _SaveFileContent(file);

                transaction.Complete();

                return file.Id;
            }
        }

        public Stream CreateZipArchive(List<Guid> fileIdsToArchive)
        {
            var fileInfos = fileIdsToArchive.Select(GetFileInfo).ToList();
            var archiveFile = new MemoryStream();

            using (var archive = new ZipArchive(archiveFile, ZipArchiveMode.Create, leaveOpen: true))
            {
                foreach (var file in fileInfos)
                {
                    var fileContent = _GetFileContent(file);
                    var archiveEntry = archive.CreateEntry(file.FileNameWithExtension, System.IO.Compression.CompressionLevel.Fastest);

                    using (var zippedStreamEntry = archiveEntry.Open())
                    {
                        fileContent.CopyTo(zippedStreamEntry);
                    }
                }
            }

            archiveFile.Seek(0, SeekOrigin.Begin);
            return archiveFile;
        }

        private void _SaveFileContent(SharedFolderFile file)
        {
            _LogInfo($"As we are using Attachment Microservice, Create Directory Functionality is not Required");
            if (file.AttachmentId.HasValue)
            {
                byte[] fileContent;
                using (BinaryReader br = new BinaryReader(file.FileContent))
                {
                    fileContent = br.ReadBytes((int)file.FileContent.Length);
                }
                var storeResult = _AttachmentMicroServiceApiClient.StoreAttachment(file.AttachmentId.Value, file.FileName, fileContent);
                _LogInfo($"Attachment is stored with AttachementId = '{file.AttachmentId}', Success = '{storeResult.success}', Message = '{storeResult.message}' ");
            }
            else
            {
                throw new InvalidOperationException($"Attempting to store a file without registering it first.  FileName='{file.FileNameWithExtension}';");
            }
        }

        private Stream _GetFileContent(SharedFolderFile file)
        {
            if (file.AttachmentId.HasValue)
            {
                var retriveResult = _AttachmentMicroServiceApiClient.RetrieveAttachment(file.AttachmentId.Value);
                var memoryStream = new MemoryStream(retriveResult.result);
                memoryStream.Seek(0, SeekOrigin.Begin);
                return memoryStream;
            }
            else
            {
                throw new InvalidOperationException($"There is no file with attachment id: {file.AttachmentId}");
            }
           
        }

        private void _RemoveFileContent(SharedFolderFile file)
        {
            if (file.AttachmentId.HasValue)
            {
                var deleteResult = _AttachmentMicroServiceApiClient.DeleteAttachment(file.AttachmentId.Value);
                _LogInfo($"Attachment is deleted with AttachementId = '{file.AttachmentId}', Success = '{deleteResult.success}', Message = '{deleteResult.message}' ");
            }
            else
            {
                _LogWarning($"Attempting to remove a file that did not have an AttachementId.  FileName='{file.FileNameWithExtension}';");
            }
        }

        public void RemoveFileFromFileShare(Guid fileId)
        {
            var file = GetFileInfo(fileId);

            using (var transaction = TransactionScopeHelper.CreateTransactionScopeWrapper(TimeSpan.FromMinutes(20)))
            {
                _RemoveFileContent(file);
                transaction.Complete();
            }
        }
    }
}
