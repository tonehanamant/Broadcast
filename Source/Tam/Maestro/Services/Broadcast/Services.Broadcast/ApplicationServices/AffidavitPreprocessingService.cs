using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using Common.Services;
using Tam.Maestro.Common;
using Tam.Maestro.Services.Cable.SystemComponentParameters;
using Services.Broadcast.ApplicationServices.Helpers;
using Services.Broadcast.Entities.Enums;

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
        List<WWTVOutboundFileValidationResult> ProcessFiles(string userName);
        List<WWTVOutboundFileValidationResult> ValidateFiles(List<string> filepathList, string userName);        
    }

    public class AffidavitPreprocessingService : IAffidavitPreprocessingService
    {
        private readonly IAffidavitRepository _AffidavitRepository;
        private readonly IDataRepositoryFactory _BroadcastDataRepositoryFactory;
        private readonly IWWTVEmailProcessorService _affidavitEmailProcessorService;
        private readonly IEmailerService _EmailerService;
        private readonly IFileService _FileService;

        private readonly IWWTVFtpHelper _WWTVFtpHelper;
        private readonly IWWTVSharedNetworkHelper _WWTVSharedNetworkHelper;

        public AffidavitPreprocessingService(IDataRepositoryFactory broadcastDataRepositoryFactory,
                                                IWWTVEmailProcessorService affidavitEmailProcessorService,
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
        public List<WWTVOutboundFileValidationResult> ProcessFiles(string userName)
        {
            List<string> filepathList;
            List<WWTVOutboundFileValidationResult> validationList = new List<WWTVOutboundFileValidationResult>();
            _WWTVSharedNetworkHelper.Impersonate(delegate
            {
                filepathList = _FileService.GetFiles(WWTVSharedNetworkHelper.GetLocalDropFolder());
                validationList = ValidateFiles(filepathList, userName);
                _AffidavitRepository.SaveValidationObject(validationList);
                var validFileList = validationList.Where(v => v.Status == FileProcessingStatusEnum.Valid)
                    .ToList();
                if (validFileList.Any())
                {
                    _CreateAndUploadZipArchiveToWWTV(validFileList.Select(x => x.FilePath).ToList());
                }

                _affidavitEmailProcessorService.ProcessAndSendInvalidDataFiles(validationList);
                _FileService.Delete(validationList.Where(x => x.Status == FileProcessingStatusEnum.Valid).Select(x => x.FilePath).ToArray());
            });

            return validationList;
        }

        /// <summary>
        /// Creates and uploads a zip archive to WWTV FTP server
        /// </summary>
        /// <param name="files">List of OutboundAffidavitFileValidationResultDto objects representing the valid files to be sent</param>
        private void _CreateAndUploadZipArchiveToWWTV(List<string> filePaths)
        {
            string zipFileName = WWTVSharedNetworkHelper.GetLocalErrorFolder();
            if (!zipFileName.EndsWith("\\"))
                zipFileName += "\\";
            zipFileName += "Post_" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".zip";
            _FileService.CreateZipArchive(filePaths, zipFileName);
            if (_FileService.Exists(zipFileName))
            {
                _UploadZipToWWTV(zipFileName);
                _FileService.Delete(zipFileName);
            }
        }

        private void _UploadZipToWWTV(string zipFileName)
        {
            var sharedFolder = _WWTVFtpHelper.GetRemoteFullPath(BroadcastServiceSystemParameter.WWTV_FtpOutboundFolder);
            var uploadUrl = $"{sharedFolder}/{Path.GetFileName(zipFileName)}";
            _WWTVFtpHelper.UploadFile(zipFileName, uploadUrl, File.Delete);
        }

        public List<WWTVOutboundFileValidationResult> ValidateFiles(List<string> filepathList, string userName)
        {
            List<WWTVOutboundFileValidationResult> result = new List<WWTVOutboundFileValidationResult>();

            foreach (var filepath in filepathList)
            {
                WWTVOutboundFileValidationResult currentFile = new WWTVOutboundFileValidationResult()
                {
                    FilePath = filepath,
                    FileName = Path.GetFileName(filepath),
                    SourceId = (int)AffidavitFileSourceEnum.Strata,
                    CreatedBy = userName,
                    CreatedDate = DateTime.Now,
                    FileHash = HashGenerator.ComputeHash(File.ReadAllBytes(filepath)),
                    Status = FileProcessingStatusEnum.Invalid
                };
                result.Add(currentFile);

                var fileInfo = new FileInfo(filepath);

                using (var affidavitPickupValidationService =
                    AffidavitPickupFileValidation.GetAffidavitValidationService(fileInfo, currentFile))
                {
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
                    {
                        currentFile.Status = FileProcessingStatusEnum.Valid;
                    }
                }
            }

            return result;
        }
    }
}
