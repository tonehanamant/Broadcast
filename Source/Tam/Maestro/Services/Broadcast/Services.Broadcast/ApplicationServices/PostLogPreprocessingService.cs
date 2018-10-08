﻿using Common.Services;
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
        private readonly IKeepingTracConverter _KeepingTracConverter;

        private readonly IFileTransferEmailHelper _EmailHelper;
        private readonly string _CsvFileExtension = ".csv";

        public PostLogPreprocessingService(IDataRepositoryFactory broadcastDataRepositoryFactory,
                                           IWWTVSharedNetworkHelper WWTVSharedNetworkHelper,
                                           IEmailerService emailerService,
                                           IFileService fileService,
                                           IWWTVFtpHelper ftpHelper,
                                           ISigmaConverter sigmaConverter,
                                           IKeepingTracConverter keepingTracConverter,
                                           IFileTransferEmailHelper emailHelper)
        {
            _DataRepositoryFactory = broadcastDataRepositoryFactory;
            _WWTVSharedNetworkHelper = WWTVSharedNetworkHelper;
            _EmailerService = emailerService;
            _FileService = fileService;
            _WWTVFtpHelper = ftpHelper;
            _SigmaConverter = sigmaConverter;
            _KeepingTracConverter = keepingTracConverter;
            _EmailHelper = emailHelper;
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

        public List<WWTVOutboundFileValidationResult> ValidateFiles(List<string> filePathList, string userName, FileSourceEnum fileSource)
        {
            var results = new List<WWTVOutboundFileValidationResult>();

            foreach (var filePath in filePathList)
            {
                WWTVOutboundFileValidationResult currentFileResult = new WWTVOutboundFileValidationResult()
                {
                    FilePath = filePath,
                    FileName = Path.GetFileName(filePath),
                    Source = FileSourceEnum.Unknown,
                    CreatedBy = userName,
                    CreatedDate = DateTime.Now,
                    FileHash = HashGenerator.ComputeHash(File.ReadAllBytes(filePath)),
                    Status = FileProcessingStatusEnum.Valid
                };
                results.Add(currentFileResult);

                var fileInfo = new FileInfo(filePath);

                if (fileInfo.Extension.Equals(_CsvFileExtension, StringComparison.InvariantCultureIgnoreCase))
                {
                    currentFileResult.Source = fileSource;
                    currentFileResult.ErrorMessages.AddRange(
                        fileSource == FileSourceEnum.Sigma 
                        ? _SigmaConverter.GetValidationResults(filePath) 
                        : _KeepingTracConverter.GetValidationResults(filePath));
                }
                else
                {
                    currentFileResult.ErrorMessages.Add($"Unknown PostLog file type for file: {filePath}");
                }

                if (currentFileResult.ErrorMessages.Any())
                    currentFileResult.Status = FileProcessingStatusEnum.Invalid;
            }

            return results;

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
