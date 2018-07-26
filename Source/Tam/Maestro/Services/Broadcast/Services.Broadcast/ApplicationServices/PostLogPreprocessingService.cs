using Common.Services;
using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.Converters;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Tam.Maestro.Common;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace Services.Broadcast.ApplicationServices
{
    public interface IPostLogPreprocessingService: IApplicationService
    {
        void ProcessFiles(string username);
        List<OutboundPostLogFileValidationResult> ValidateFiles(List<string> filePathList, string userName);
    }

    public interface IPostLogPreprocessingValidator
    {
        List<string> GetValidationResults(string filePath);
    }
    class PostLogPreprocessingService: IPostLogPreprocessingService
    {
        private readonly IDataRepositoryFactory _DataRepositoryFactory;
        private readonly IWWTVSharedNetworkHelper _WWTVSharedNetworkHelper;
        private readonly IFileService _FileService;
        private readonly IEmailerService _EmailerService;
        private readonly IWWTVFtpHelper _WWTVFtpHelper;
        private readonly string _SigmaFileExtension = ".csv";

        public PostLogPreprocessingService(IDataRepositoryFactory broadcastDataRepositoryFactory,
                                           IWWTVSharedNetworkHelper WWTVSharedNetworkHelper,
                                           IEmailerService emailerService,
                                           IFileService fileService,
                                           IWWTVFtpHelper ftpHelper)
        {
            _DataRepositoryFactory = broadcastDataRepositoryFactory;
            _WWTVSharedNetworkHelper = WWTVSharedNetworkHelper;
            _EmailerService = emailerService;
            _FileService = fileService;
            _WWTVFtpHelper = ftpHelper;
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

                _DeleteFiles(validFileList);
            });
        }

        private void _MoveToErrorFolderAndSendNotification(List<OutboundPostLogFileValidationResult> invalidFileList)
        {

            foreach (var invalidFile in invalidFileList)
            {
                var invalidFilePath = _MoveInvalidFileToArchiveFolder(invalidFile);

                var emailBody = _CreateInvalidDataFileEmailBody(invalidFile, invalidFilePath);

                _Send(emailBody, "Error Preprocessing");
            }
        }

        private string _MoveInvalidFileToArchiveFolder(OutboundPostLogFileValidationResult invalidFile)
        {
            var errorFolder = BroadcastServiceSystemParameter.WWTV_PostLogErrorFolder;
            var destinationPath = Path.Combine(errorFolder, Path.GetFileName(invalidFile.FilePath));

            if (File.Exists(destinationPath))
                File.Delete(destinationPath);

            File.Move(invalidFile.FilePath, destinationPath);

            return destinationPath;
        }

        private string _CreateInvalidDataFileEmailBody(OutboundPostLogFileValidationResult invalidFile, string invalidFilePath)
        {
            var mailBody = new StringBuilder();

            mailBody.AppendFormat("File {0} failed validation for WWTV upload\n\n", invalidFile.FileName);

            foreach (var errorMessage in invalidFile.ErrorMessages)
            {
                mailBody.Append(string.Format("{0}\n", errorMessage));
            }

            mailBody.AppendFormat("\nFile located in {0}\n", invalidFilePath);

            return mailBody.ToString();
        }

        private void _Send(string emailBody, string subject)
        {
            if (!BroadcastServiceSystemParameter.EmailNotificationsEnabled)
                return;

            var from = new MailAddress(BroadcastServiceSystemParameter.EmailUsername);
            var to = new List<MailAddress>() { new MailAddress(BroadcastServiceSystemParameter.WWTV_NotificationEmail) };
            _EmailerService.QuickSend(false, emailBody, subject, MailPriority.Normal, from, to);
        }

        private static void _DeleteFiles(List<OutboundPostLogFileValidationResult> validationList)
        {
            validationList.ForEach(r =>
            {
                    File.Delete(r.FilePath);
            });
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
                IPostLogPreprocessingValidator postLogValidator = null;
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
                    postLogValidator = new SigmaConverter();
                }

                if(postLogValidator == null)
                {
                    currentFileResult.ErrorMessages.Add($"Unknown PostLog file type for file: {filePath}");
                }
                else
                {
                    currentFileResult.ErrorMessages.AddRange(postLogValidator.GetValidationResults(filePath));
                }

                if (currentFileResult.ErrorMessages.Any())
                    currentFileResult.Status = PostLogProcessingStatusEnum.Invalid;
            }

            return results;

        }

        private List<string> _GetDropFolderFileList()
        {
            var dropFolder = "";
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
