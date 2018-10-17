using Common.Services;
using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.ApplicationServices.Helpers;
using Services.Broadcast.Converters;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tam.Maestro.Common;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace Services.Broadcast.ApplicationServices
{
    public interface IPostLogPreprocessingService : IApplicationService
    {
        void ProcessFiles(string username);
        List<WWTVOutboundFileValidationResult> ValidateFiles(List<string> filePathList, string userName, FileSourceEnum fileSource);
    }

    class PostLogPreprocessingService : IPostLogPreprocessingService
    {
        private readonly IDataRepositoryFactory _DataRepositoryFactory;
        private readonly IPostLogRepository _PostLogRepository;
        private readonly IWWTVSharedNetworkHelper _WWTVSharedNetworkHelper;
        private readonly IFileService _FileService;
        private readonly IEmailerService _EmailerService;
        private readonly IWWTVFtpHelper _WWTVFtpHelper;
        private readonly ISigmaConverter _SigmaConverter;
        private readonly IExcelHelper _ExcelHelper;

        private readonly IFileTransferEmailHelper _EmailHelper;
        private readonly string _CsvFileExtension = ".csv";
        private readonly string _ExcelFileExtension = ".xlsx";

        public PostLogPreprocessingService(IDataRepositoryFactory broadcastDataRepositoryFactory
                                           , IWWTVSharedNetworkHelper WWTVSharedNetworkHelper
                                           , IEmailerService emailerService
                                           , IFileService fileService
                                           , IWWTVFtpHelper ftpHelper
                                           , ISigmaConverter sigmaConverter
                                           , IFileTransferEmailHelper emailHelper
                                           , IExcelHelper excelHelper)
        {
            _DataRepositoryFactory = broadcastDataRepositoryFactory;
            _WWTVSharedNetworkHelper = WWTVSharedNetworkHelper;
            _EmailerService = emailerService;
            _FileService = fileService;
            _WWTVFtpHelper = ftpHelper;
            _SigmaConverter = sigmaConverter;
            _EmailHelper = emailHelper;
            _ExcelHelper = excelHelper;
            _PostLogRepository = _DataRepositoryFactory.GetDataRepository<IPostLogRepository>();
        }

        public void ProcessFiles(string username)
        {
            _WWTVSharedNetworkHelper.Impersonate(delegate
            {
                var dropFilePathList = _FileService.GetFiles(BroadcastServiceSystemParameter.WWTV_PostLogDropFolder);
                var validationResults = ValidateFiles(dropFilePathList, username, FileSourceEnum.Sigma);
                _SaveAndUploadToWWTV(validationResults, FileSourceEnum.Sigma);

                var ktDropFilePathList = _FileService.GetFiles(BroadcastServiceSystemParameter.WWTV_KeepingTracDropFolder);
                var ktValidationResults = ValidateFiles(ktDropFilePathList, username, FileSourceEnum.KeepingTrac);
                _SaveAndUploadToWWTV(ktValidationResults, FileSourceEnum.KeepingTrac);
            });
        }

        public List<WWTVOutboundFileValidationResult> ValidateFiles(List<string> filePathList, string userName, FileSourceEnum fileSource)
        {
            var results = new List<WWTVOutboundFileValidationResult>();

            foreach (var filePath in filePathList)
            {
                WWTVOutboundFileValidationResult currentFile = new WWTVOutboundFileValidationResult()
                {
                    FilePath = filePath,
                    FileName = Path.GetFileName(filePath),
                    CreatedBy = userName,
                    CreatedDate = DateTime.Now,
                    FileHash = HashGenerator.ComputeHash(File.ReadAllBytes(filePath)),
                    Status = FileProcessingStatusEnum.Valid                    
                };
                results.Add(currentFile);

                var fileInfo = new FileInfo(filePath);

                if (fileSource.Equals(FileSourceEnum.Sigma) && fileInfo.Extension.Equals(_CsvFileExtension, StringComparison.InvariantCultureIgnoreCase))
                {
                    currentFile.Source = FileSourceEnum.Sigma;
                    currentFile.ErrorMessages.AddRange(_SigmaConverter.GetValidationResults(filePath));
                }
                else if (fileSource.Equals(FileSourceEnum.KeepingTrac) && fileInfo.Extension.Equals(_ExcelFileExtension, StringComparison.InvariantCultureIgnoreCase))
                {
                    currentFile.Source = FileSourceEnum.KeepingTrac;
                    _LoadKeepingTracValidationResults(filePath, currentFile);
                }
                else
                {                    
                    currentFile.ErrorMessages.Add($"Unknown PostLog file type for file: {filePath}");
                }

                if (currentFile.ErrorMessages.Any())
                {
                    currentFile.Source = FileSourceEnum.Unknown;
                    currentFile.Status = FileProcessingStatusEnum.Invalid;
                }                    
            }

            return results;

        }

        private void _LoadKeepingTracValidationResults(string filePath, WWTVOutboundFileValidationResult currentFile)
        {
            List<string> requiredColumns = new List<string>() { "Estimate", "Station", "Air Date", "Air Time", "Air ISCI", "Demographic", "Act Ratings", "Act Impression" };
            FileInfo fileInfo = new FileInfo(filePath);

            //get the tab that needs processing
            var worksheet = _ExcelHelper.GetWorksheetToProcess(fileInfo, currentFile);            
            if (worksheet == null)
            {
                currentFile.Source = FileSourceEnum.Unknown;
                currentFile.ErrorMessages.Add(string.Format("Could not find the tab in file {0}", currentFile.FilePath));
                return;
            }
            
            //check column headers
            var fileColumns = _ExcelHelper.ValidateHeaders(requiredColumns, worksheet, currentFile);
            if (currentFile.ErrorMessages.Any())
                return;

            //check required data fields
            _ExcelHelper.CheckMissingDataOnRequiredColumns(requiredColumns, fileColumns, worksheet, currentFile);
            if (currentFile.ErrorMessages.Any())
                return;
        }

        private void _SaveAndUploadToWWTV(List<WWTVOutboundFileValidationResult> validationResults, FileSourceEnum source)
        {
            _PostLogRepository.SavePreprocessingValidationResults(validationResults);
            var validFileList = validationResults.Where(v => v.Status == FileProcessingStatusEnum.Valid)
                .ToList();
            if (validFileList.Any())
            {
                _CreateAndUploadZipArchiveToWWTV(validFileList.Select(x => x.FilePath).ToList());
            }

            var invalidFileList = validationResults.Where(v => v.Status == FileProcessingStatusEnum.Invalid)
                .ToList();
            if (invalidFileList.Any())
            {
                _MoveToErrorFolderAndSendNotification(invalidFileList, source);
            }

            _FileService.Delete(validFileList.Select(x => x.FilePath).ToArray());
        }

        private void _MoveToErrorFolderAndSendNotification(List<WWTVOutboundFileValidationResult> invalidFileList, FileSourceEnum source)
        {
            foreach (var invalidFile in invalidFileList)
            {
                var invalidFilePath = _FileService.Move(invalidFile.FilePath,
                    source.Equals(FileSourceEnum.KeepingTrac) ? BroadcastServiceSystemParameter.WWTV_KeepingTracErrorFolder : BroadcastServiceSystemParameter.WWTV_PostLogErrorFolder);

                var emailBody = _EmailHelper.CreateInvalidDataFileEmailBody(invalidFile.ErrorMessages, invalidFilePath, invalidFile.FileName);

                _EmailHelper.SendEmail(emailBody, "Error Preprocessing");
            }
        }

        private void _CreateAndUploadZipArchiveToWWTV(List<string> filePaths)
        {
            string zipFileName = BroadcastServiceSystemParameter.WWTV_KeepingTracErrorFolder;
            if (!zipFileName.EndsWith("\\"))
                zipFileName += "\\";
            zipFileName += "PostLog_" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".zip";
            _FileService.CreateZipArchive(filePaths, zipFileName);
            if (_FileService.Exists(zipFileName))
            {
                _WWTVFtpHelper.UploadFile(zipFileName, $"{_WWTVFtpHelper.GetRemoteFullPath(BroadcastServiceSystemParameter.WWTV_KeepingTracFtpOutboundFolder)}/{Path.GetFileName(zipFileName)}", File.Delete);
                _FileService.Delete(zipFileName);
            }
        }
    }
}
