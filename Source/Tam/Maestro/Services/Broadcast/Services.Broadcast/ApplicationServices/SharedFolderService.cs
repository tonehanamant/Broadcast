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
        private readonly IFileService _FileService;
        private readonly ISharedFolderFilesRepository _SharedFolderFilesRepository;
        private readonly IAttachmentMicroServiceApiClient _AttachmentMicroServiceApiClient;
        protected Lazy<bool> _IsAttachementMicroServiceEnabled;

        public SharedFolderService(
            IFileService fileService,
            IDataRepositoryFactory broadcastDataRepositoryFactory, IAttachmentMicroServiceApiClient attachmentMicroServiceApiClient, IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper)
            : base(featureToggleHelper, configurationSettingsHelper)
        {
            _FileService = fileService;
            _SharedFolderFilesRepository = broadcastDataRepositoryFactory.GetDataRepository<ISharedFolderFilesRepository>();
            _AttachmentMicroServiceApiClient = attachmentMicroServiceApiClient;
            _IsAttachementMicroServiceEnabled = new Lazy<bool>(() =>
               _FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_ATTACHMENT_MICRO_SERVICE));

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
                if (_IsAttachementMicroServiceEnabled.Value)
                {
                    var registerResult = _AttachmentMicroServiceApiClient.RegisterAttachment(file.FileName, file.CreatedBy, "");
                    if (registerResult != null)
                    {
                        file.AttachmentId = registerResult.AttachmentId;
                    }
                    _LogInfo($"Attachment is registered with AttachementId = '{file.AttachmentId}'");
                }
                
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

            if (_IsAttachementMicroServiceEnabled.Value)
            {
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

            // this is if we have a file system.
            var filePaths = fileInfos.ToDictionary(f => Path.Combine(f.FolderPath, f.Id.ToString()), f => f.FileNameWithExtension);            
            using (var archive = new ZipArchive(archiveFile, ZipArchiveMode.Create, leaveOpen: true))
            {
                foreach (var pair in filePaths)
                {
                    archive.CreateEntryFromFile(pair.Key, pair.Value, CompressionLevel.Fastest);
                }
            }
            archiveFile.Seek(0, SeekOrigin.Begin);
            return archiveFile;
        }        

        private void _SaveFileContent(SharedFolderFile file)
        {
            if (_IsAttachementMicroServiceEnabled.Value)
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
            else
            {
                _CreateDirectory(file.FolderPath);

                var sharedFolderFileName = _GetSharedFolderFileName(file.Id, file.FileExtension);
                _FileService.Create(file.FolderPath, sharedFolderFileName, file.FileContent);
            }
        }

        private void _CreateDirectory(string directoryPath)
        {
            _FileService.CreateDirectory(directoryPath);
        }

        private Stream _GetFileContent(SharedFolderFile file)
        {
            if (_IsAttachementMicroServiceEnabled.Value)
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
            else
            {
                var sharedFolderFileName = _GetSharedFolderFileName(file.Id, file.FileExtension);
                return _FileService.GetFileStream(file.FolderPath, sharedFolderFileName);
            }
        }

        private void _RemoveFileContent(SharedFolderFile file)
        {
            if (_IsAttachementMicroServiceEnabled.Value)
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
            else
            {
                var sharedFolderFileName = _GetSharedFolderFileName(file.Id, file.FileExtension);
                var fullName = $@"{file.FolderPath}\{sharedFolderFileName}";
                _FileService.Delete(fullName);
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

        // we use fileId as a file name so that we can store files with the same name
        private string _GetSharedFolderFileName(Guid fileId, string fileExtension) => fileId + fileExtension;
    }
}
