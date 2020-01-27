using Common.Services;
using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using System;
using System.IO;

namespace Services.Broadcast.ApplicationServices
{
    public interface ISharedFolderService : IApplicationService
    {
        Guid SaveFile(SharedFolderFile file);

        SharedFolderFile GetFile(Guid fileId);

        void RemoveFile(Guid fileId);

        SharedFolderFile GetAndRemoveFile(Guid fileId);
    }

    public class SharedFolderService : ISharedFolderService
    {
        private readonly IFileService _FileService;
        private readonly ISharedFolderFilesRepository _SharedFolderFilesRepository;

        public SharedFolderService(
            IFileService fileService,
            IDataRepositoryFactory broadcastDataRepositoryFactory)
        {
            _FileService = fileService;
            _SharedFolderFilesRepository = broadcastDataRepositoryFactory.GetDataRepository<ISharedFolderFilesRepository>();
        }

        public SharedFolderFile GetAndRemoveFile(Guid fileId)
        {
            var file = GetFile(fileId);

            RemoveFile(fileId);

            return file;
        }

        public SharedFolderFile GetFile(Guid fileId)
        {
            var file = _GetFileInfo(fileId);
            file.FileContent = _GetFileContent(file);

            return file;
        }

        public void RemoveFile(Guid fileId)
        {
            var file = _GetFileInfo(fileId);

            using (var transaction = TransactionScopeHelper.CreateTransactionScopeWrapper(TimeSpan.FromMinutes(20)))
            {
                _SharedFolderFilesRepository.RemoveFile(fileId);
                _RemoveFileContent(file);
            }
        }

        public Guid SaveFile(SharedFolderFile file)
        {
            using (var transaction = TransactionScopeHelper.CreateTransactionScopeWrapper(TimeSpan.FromMinutes(20)))
            {
                file.Id = _SharedFolderFilesRepository.SaveFile(file);
                _SaveFileContent(file);
                
                transaction.Complete();

                return file.Id;
            }
        }

        private SharedFolderFile _GetFileInfo(Guid fileId)
        {
            var file = _SharedFolderFilesRepository.GetFileById(fileId);

            if (file == null)
                throw new Exception($"There is no file with id: {fileId}");

            return file;
        }

        private void _SaveFileContent(SharedFolderFile file)
        {
            var sharedFolderFileName = _GetSharedFolderFileName(file.Id, file.FileExtension);
            _FileService.Create(file.FolderPath, sharedFolderFileName, file.FileContent);
        }

        private Stream _GetFileContent(SharedFolderFile file)
        {
            var sharedFolderFileName = _GetSharedFolderFileName(file.Id, file.FileExtension);
            return _FileService.GetFileStream(file.FolderPath, sharedFolderFileName);
        }

        private void _RemoveFileContent(SharedFolderFile file)
        {
            var sharedFolderFileName = _GetSharedFolderFileName(file.Id, file.FileExtension);
            var fullName = $@"{file.FolderPath}\{sharedFolderFileName}";
            _FileService.Delete(fullName);
        }
        
        // we use fileId as a file name so that we can store files with the same name
        private string _GetSharedFolderFileName(Guid fileId, string fileExtension) => fileId + fileExtension;
    }
}
