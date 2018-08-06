using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Newtonsoft.Json;
using OfficeOpenXml;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using Common.Services;
using Tam.Maestro.Common;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace Services.Broadcast.ApplicationServices
{
    public interface IAffidavitPreprocessingService : IApplicationService
    {
        /// <summary>
        /// Checks if all the files are valid according to Strata file validation rules
        /// </summary>
        /// <param name="filepathList">List of filepaths</param>
        /// <param name="userName">User processing the files</param>
        /// <returns>List of OutboundAffidavitFileValidationResultDto objects</returns>
        List<OutboundAffidavitFileValidationResultDto> ProcessFiles(string userName);
        List<OutboundAffidavitFileValidationResultDto> ValidateFiles(List<string> filepathList, string userName);
        void ProcessErrorFiles();
    }

    public class AffidavitPreprocessingService : IAffidavitPreprocessingService
    {
        private readonly IAffidavitRepository _AffidavitRepository;
        private readonly IDataRepositoryFactory _BroadcastDataRepositoryFactory;
        private readonly IAffidavitEmailProcessorService _affidavitEmailProcessorService;
        private readonly IEmailerService _EmailerService;
        private readonly IFileService _FileService;

        private readonly IWWTVFtpHelper _WWTVFtpHelper;
        private readonly IWWTVSharedNetworkHelper _WWTVSharedNetworkHelper;

        public AffidavitPreprocessingService(IDataRepositoryFactory broadcastDataRepositoryFactory, 
                                                IAffidavitEmailProcessorService affidavitEmailProcessorService,
                                                IWWTVFtpHelper WWTVFtpHelper,
                                                IWWTVSharedNetworkHelper WWTVSharedNetworkHelper,
                                                IEmailerService emailerService,
                                                IFileService fileService)
        {
            _BroadcastDataRepositoryFactory = broadcastDataRepositoryFactory;
            _AffidavitRepository = _BroadcastDataRepositoryFactory.GetDataRepository<IAffidavitRepository>();
            _affidavitEmailProcessorService = affidavitEmailProcessorService;
            _WWTVFtpHelper = WWTVFtpHelper;
            _EmailerService = emailerService;
            _WWTVSharedNetworkHelper = WWTVSharedNetworkHelper;
            _FileService = fileService;
        }

        /// <summary>
        /// Checks if all the files are valid according to Strata file validation rules
        /// </summary>
        /// <param name="filepathList">List of filepaths</param>
        /// <param name="userName">User processing the files</param>
        /// <returns>List of ValidationFileResponseDto objects</returns>
        public List<OutboundAffidavitFileValidationResultDto> ProcessFiles(string userName)
        {
            List<string> filepathList;
            List<OutboundAffidavitFileValidationResultDto> validationList = new List<OutboundAffidavitFileValidationResultDto>();
            _WWTVSharedNetworkHelper.Impersonate(delegate 
            {
                filepathList = GetDropFolderFileList();
                validationList = ValidateFiles(filepathList, userName);
                _AffidavitRepository.SaveValidationObject(validationList);
                var validFileList = validationList.Where(v => v.Status == AffidaviteFileProcessingStatus.Valid)
                    .ToList();
                if (validFileList.Any())
                {
                    this._CreateAndUploadZipArchiveToWWTV(validFileList);
                }

                _affidavitEmailProcessorService.ProcessAndSendInvalidDataFiles(validationList);
                DeleteSuccessfulFiles(validationList);
            });

            return validationList;
        }

        private List<string> GetDropFolderFileList()
        {
            List<string> filepathList;
            try
            {
                var dropFolder = WWTVSharedNetworkHelper.GetLocalDropFolder();
                filepathList = _FileService.GetFiles(dropFolder).ToList();
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Could not find WWTV_SharedFolder.  Please check it is created.", e);
            }

            return filepathList;
        }

        private static void DeleteSuccessfulFiles(List<OutboundAffidavitFileValidationResultDto> validationList)
        {
            validationList.ForEach(r =>
            {
                if (r.Status == AffidaviteFileProcessingStatus.Valid)
                {
                    File.Delete(r.FilePath);
                }
            });
        }


        /// <summary>
        /// Creates and uploads a zip archive to WWTV FTP server
        /// </summary>
        /// <param name="files">List of OutboundAffidavitFileValidationResultDto objects representing the valid files to be sent</param>
        private void _CreateAndUploadZipArchiveToWWTV(List<OutboundAffidavitFileValidationResultDto> files)
        {
            string zipFileName = WWTVSharedNetworkHelper.GetLocalErrorFolder();
            if (!zipFileName.EndsWith("\\"))
                zipFileName += "\\";
            zipFileName += "Post_" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".zip";
            _CreateZipArchive(files, zipFileName);
            if (File.Exists(zipFileName))
            {
                _UploadZipToWWTV(zipFileName);
                File.Delete(zipFileName);
            }
        }


        public void ProcessErrorFiles()
        {
            _WWTVSharedNetworkHelper.Impersonate(delegate
            {
                var files = _WWTVFtpHelper.GetFtpErrorFileList();
                var localPaths = new List<string>();
                var remoteFTPPath = _WWTVFtpHelper.GetErrorPath();

                var completedFiles = _DownloadFTPFiles(files, remoteFTPPath, ref localPaths);
                EmailFTPErrorFiles(localPaths);
            });
        }

        private void EmailFTPErrorFiles(List<string> filePaths)
        {
            if (!BroadcastServiceSystemParameter.EmailNotificationsEnabled)
                return;

            filePaths.ForEach(filePath =>
            {
                var body = string.Format("{0}",Path.GetFileName(filePath));
                var subject = "Error files from WWTV";
                var from = BroadcastServiceSystemParameter.EmailUsername;
                var Tos = new string[] { BroadcastServiceSystemParameter.WWTV_NotificationEmail };
                _EmailerService.QuickSend(true, body, subject, MailPriority.Normal,from , Tos, new List<string>() {filePath });
            });
        }

        private List<string> _DownloadFTPFiles(List<string> files, string remoteFtpPath,ref List<string> localFilePaths)
        {
            var local = new List<string>();
            List<string> completedFiles = new List<string>();
            using (var ftpClient = _WWTVFtpHelper.EnsureFtpClient())
            {
                var localFolder = WWTVSharedNetworkHelper.GetLocalErrorFolder();
                files.ForEach(filePath =>
                {
                    var path = remoteFtpPath + "/" + filePath.Remove(0, filePath.IndexOf(@"/") + 1);
                    var localPath = localFolder + @"\" + filePath.Replace(@"/", @"\");
                    if (_FileService.Exists(localPath))
                        _FileService.Delete(localPath);

                    _WWTVFtpHelper.DownloadFileFromClient(ftpClient,path, localPath);
                    local.Add(localPath);
                    _WWTVFtpHelper.DeleteFile(path);
                    completedFiles.Add(path);
                });
            }

            localFilePaths = local;
            return completedFiles;
        }


        private static void _CreateZipArchive(List<OutboundAffidavitFileValidationResultDto> files, string zipFileName)
        {
            using (ZipArchive zip = ZipFile.Open(zipFileName, ZipArchiveMode.Create))
            {
                foreach (var file in files)
                {
                    // Add the entry for each file
                    zip.CreateEntryFromFile(file.FilePath, Path.GetFileName(file.FilePath), System.IO.Compression.CompressionLevel.NoCompression);
                }
            }
        }

        private void _UploadZipToWWTV(string zipFileName)
        {
            var sharedFolder = _WWTVFtpHelper.GetOutboundPath();
            var uploadUrl = $"{sharedFolder}/{Path.GetFileName(zipFileName)}";
            _WWTVFtpHelper.UploadFile(zipFileName, uploadUrl,File.Delete); 
        }


        public List<OutboundAffidavitFileValidationResultDto> ValidateFiles(List<string> filepathList, string userName)
        {
            List<OutboundAffidavitFileValidationResultDto> result = new List<OutboundAffidavitFileValidationResultDto>();

            foreach (var filepath in filepathList)
            {
                OutboundAffidavitFileValidationResultDto currentFile = new OutboundAffidavitFileValidationResultDto()
                {
                    FilePath = filepath,
                    FileName = Path.GetFileName(filepath),
                    SourceId = (int)AffidaviteFileSourceEnum.Strata,
                    CreatedBy = userName,
                    CreatedDate = DateTime.Now,
                    FileHash = HashGenerator.ComputeHash(File.ReadAllBytes(filepath))
                };
                result.Add(currentFile);

                var fileInfo = new FileInfo(filepath);

                var affidavitPickupValidationService = AffidavitPickupFileValidation.GetAffidavitValidationService(fileInfo,currentFile);
                if (currentFile.ErrorMessages.Any())
                    continue;

                //check if tab exists
                affidavitPickupValidationService.ValidateFileStruct();
                if (currentFile.ErrorMessages.Any())
                    continue;

                //check column headers
                affidavitPickupValidationService.ValidateHeaders();
                if (currentFile.ErrorMessages.Any())
                    continue;

                //check required data fields
                affidavitPickupValidationService.HasMissingData();
                if (currentFile.ErrorMessages.Any())
                    continue;

                if (!currentFile.ErrorMessages.Any())
                    currentFile.Status = AffidaviteFileProcessingStatus.Valid;
            }

            return result;
        }
    }
}
