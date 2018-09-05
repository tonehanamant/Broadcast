using Common.Services;
using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.ApplicationServices.Helpers;
using Services.Broadcast.Converters;
using Services.Broadcast.Entities;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Mail;
using System.Text;
using Tam.Maestro.Common;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace Services.Broadcast.ApplicationServices
{
    public interface IPostLogPreprocessingService: IApplicationService
    {
        void ProcessFiles(string username);
        List<OutboundPostLogFileValidationResult> ValidateFiles(List<string> filePathList, string userName);
    }

    class PostLogPreprocessingService: IPostLogPreprocessingService
    {
        private readonly IDataRepositoryFactory _DataRepositoryFactory;
        private readonly IWWTVSharedNetworkHelper _WWTVSharedNetworkHelper;
        private readonly IFileService _FileService;
        private readonly IEmailerService _EmailerService;
        private readonly IWWTVFtpHelper _WWTVFtpHelper;
        private readonly ISigmaConverter _SigmaConverter;

        private readonly IFileTransferEmailHelper _EmailHelper;
        private readonly string _SigmaFileExtension = ".csv";

        public PostLogPreprocessingService(IDataRepositoryFactory broadcastDataRepositoryFactory,
                                           IWWTVSharedNetworkHelper WWTVSharedNetworkHelper,
                                           IEmailerService emailerService,
                                           IFileService fileService,
                                           IWWTVFtpHelper ftpHelper,
                                           ISigmaConverter sigmaConverter,
                                           IFileTransferEmailHelper emailHelper)
        {
            _DataRepositoryFactory = broadcastDataRepositoryFactory;
            _WWTVSharedNetworkHelper = WWTVSharedNetworkHelper;
            _EmailerService = emailerService;
            _FileService = fileService;
            _WWTVFtpHelper = ftpHelper;
            _SigmaConverter = sigmaConverter;
            _EmailHelper = emailHelper;
        }
        public void ProcessFiles(string username)
        {
            _WWTVSharedNetworkHelper.Impersonate(delegate
            {
                var dropFilePathList = _GetDropFolderFileList();
                var validationResults = ValidateFiles(dropFilePathList, username);
                _DataRepositoryFactory.GetDataRepository<IPostLogRepository>().SavePreprocessingValidationResults(validationResults);
                var validFileList = validationResults.Where(v => v.Status == PostLogProcessingStatusEnum.Valid)
                    .ToList();
                if (validFileList.Any())
                {
                    _CreateAndUploadZipArchiveToWWTV(validFileList);
                }

                var invalidFileList = validationResults.Where(v => v.Status == PostLogProcessingStatusEnum.Invalid)
                    .ToList();
                if (invalidFileList.Any())
                {
                    _MoveToErrorFolderAndSendNotification(invalidFileList);
                }

                _FileService.Delete(validFileList.Select(x=>x.FilePath).ToArray());
            });
        }

        private void _MoveToErrorFolderAndSendNotification(List<OutboundPostLogFileValidationResult> invalidFileList)
        {
            foreach (var invalidFile in invalidFileList)
            {
                var invalidFilePath = _FileService.Move(invalidFile.FilePath, BroadcastServiceSystemParameter.WWTV_PostLogErrorFolder);

                var emailBody =  _EmailHelper.CreateInvalidDataFileEmailBody(invalidFile.ErrorMessages, invalidFilePath, invalidFile.FileName);

                _EmailHelper.SendEmail(emailBody, "Error Preprocessing");
            }
        }

        private void _CreateAndUploadZipArchiveToWWTV(List<OutboundPostLogFileValidationResult> files)
        {
            string zipFileName = BroadcastServiceSystemParameter.WWTV_PostLogErrorFolder;
            if (!zipFileName.EndsWith("\\"))
                zipFileName += "\\";
            zipFileName += "PostLog_" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".zip";
            _CreateZipArchive(files, zipFileName);
            if (File.Exists(zipFileName))
            {
                _UploadZipToWWTV(zipFileName);
                File.Delete(zipFileName);
            }
        }

        private static void _CreateZipArchive(List<OutboundPostLogFileValidationResult> files, string zipFileName)
        {
            using (ZipArchive zip = ZipFile.Open(zipFileName, ZipArchiveMode.Create))
            {
                foreach (var file in files)
                {
                    // Add the entry for each file
                    zip.CreateEntryFromFile(file.FilePath, Path.GetFileName(file.FilePath), System.IO.Compression.CompressionLevel.Fastest);
                }
            }
        }

        private void _UploadZipToWWTV(string zipFileName)
        {
            var sharedFolder = BroadcastServiceSystemParameter.WWTV_PostLogFtpOutboundFolder;
            var ftpHost = BroadcastServiceSystemParameter.WWTV_FtpHost;
            var uploadUrl = $"ftp://{ftpHost}/{sharedFolder}/{Path.GetFileName(zipFileName)}";
            _WWTVFtpHelper.UploadFile(zipFileName, uploadUrl, File.Delete);
        }

        public List<OutboundPostLogFileValidationResult> ValidateFiles(List<string> filePathList, string userName)
        {
            var results = new List<OutboundPostLogFileValidationResult>();

            foreach (var filePath in filePathList)
            {
                OutboundPostLogFileValidationResult currentFileResult = new OutboundPostLogFileValidationResult()
                {
                    FilePath = filePath,
                    FileName = Path.GetFileName(filePath),
                    Source = PostLogFileSourceEnum.Unknown,
                    CreatedBy = userName,
                    CreatedDate = DateTime.Now,
                    FileHash = HashGenerator.ComputeHash(File.ReadAllBytes(filePath)),
                    Status = PostLogProcessingStatusEnum.Valid
                };
                results.Add(currentFileResult);

                var fileInfo = new FileInfo(filePath);

                if (fileInfo.Extension.Equals(_SigmaFileExtension, StringComparison.InvariantCultureIgnoreCase))
                {
                    currentFileResult.Source = PostLogFileSourceEnum.Sigma;
                    currentFileResult.ErrorMessages.AddRange(_SigmaConverter.GetValidationResults(filePath));
                }
                else
                {
                    currentFileResult.ErrorMessages.Add($"Unknown PostLog file type for file: {filePath}");
                }

                if (currentFileResult.ErrorMessages.Any())
                    currentFileResult.Status = PostLogProcessingStatusEnum.Invalid;
            }

            return results;

        }

        private List<string> _GetDropFolderFileList()
        {
            var dropFolder = string.Empty;
            List<string> filepathList;
            try
            {
                dropFolder = BroadcastServiceSystemParameter.WWTV_PostLogDropFolder;
                filepathList = _FileService.GetFiles(dropFolder).ToList();
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Unable to read from WWTV_PostLogDropFolder folder {dropFolder}.", e);
            }

            return filepathList;
        }
    }
}
