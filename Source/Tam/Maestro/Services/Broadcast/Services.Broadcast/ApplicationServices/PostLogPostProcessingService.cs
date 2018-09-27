﻿using Common.Services;
using Common.Services.ApplicationServices;
using Services.Broadcast.ApplicationServices.Helpers;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace Services.Broadcast.ApplicationServices
{
    public interface IPostLogPostProcessingService : IApplicationService
    {
        /// <summary>
        /// Downloads the WWTV processed files and calls the postlog processing service
        /// return true if download success, false if download fails (use for loggin)
        /// This involves FTP 
        /// </summary>        
        DownloadAndProcessWWTVFilesResponse DownloadAndProcessWWTVFiles(string userName);

        /// <summary>
        /// Processes the file content
        /// </summary>
        /// <param name="userName">Username requesting the processing</param>
        /// <param name="fileName">FIlename to be processed</param>
        /// <param name="fileContents">File content as string</param>
        /// <returns>WWTVSaveResult object</returns>
        WWTVSaveResult ProcessFileContents(string userName, string fileName, string fileContents);

        /// <summary>
        /// Process error files created by WWTV based on files uploaded by us
        /// </summary>
        void ProcessErrorFiles();
    }

    public class PostLogPostProcessingService : BasePostProcessingService, IPostLogPostProcessingService
    {
        private readonly IWWTVFtpHelper _WWTVFtpHelper;
        private readonly IWWTVEmailProcessorService _EmailProcessorService;
        private readonly IPostLogService _PostLogService;
        private readonly IWWTVSharedNetworkHelper _WWTVSharedNetworkHelper;
        private readonly IEmailerService _EmailerService;
        private readonly IFileService _FileService;

        public PostLogPostProcessingService(IWWTVFtpHelper wwtvFtpHelper
            , IWWTVEmailProcessorService emailService
            , IPostLogService postLogService
            , IFileTransferEmailHelper emailHelper
            , IBroadcastAudiencesCache audienceCache
            , IWWTVSharedNetworkHelper wWTVSharedNetworkHelper
            , IEmailerService emailerService
            , IFileService fileService) : base(emailHelper, wwtvFtpHelper, audienceCache, emailerService, fileService)
        {
            _WWTVFtpHelper = wwtvFtpHelper;
            _EmailProcessorService = emailService;
            _PostLogService = postLogService;
            _WWTVSharedNetworkHelper = wWTVSharedNetworkHelper;
            _EmailerService = emailerService;
            _FileService = fileService;
        }

        /// <summary>
        /// Downloads the WWTV processed files and calls the postlog processing service
        /// return true if download success, false if download fails (use for loggin)
        /// This involves FTP 
        /// </summary>
        public DownloadAndProcessWWTVFilesResponse DownloadAndProcessWWTVFiles(string userName)
        {
            var inboundFile = BroadcastServiceSystemParameter.WWTV_KeepingTracFtpInboundFolder;
            if (!inboundFile.EndsWith("/"))
                inboundFile += "/";

            List<string> filesToProcess = DownloadFilesToBeProcessed(inboundFile);
            var response = new DownloadAndProcessWWTVFilesResponse();
            if (!filesToProcess.Any())
            {
                return response;
            }
            response.FilesFoundToProcess.AddRange(filesToProcess);

            var inboundFtpPath = _WWTVFtpHelper.GetRemoteFullPath(inboundFile);

            foreach (var fileName in filesToProcess)
            {
                var fullFtpPath = inboundFile + fileName;
                //download file content
                string fileContents = _WWTVFtpHelper.DownloadFTPFileContent(fullFtpPath, out bool success, out string errorMessage);
                if (!success)
                {
                    response.FailedDownloads.Add(fileName + " Reason: " + errorMessage);
                    continue;
                }

                try
                {
                    _WWTVFtpHelper.DeleteFiles(fullFtpPath);
                }
                catch (Exception e)
                {
                    var errorDeletingFile = "Error deleting post log file from FTP site: " + fullFtpPath + "\r\n" + e;
                    _EmailProcessorService.ProcessAndSendTechError(fileName, errorMessage, fileContents);
                    continue;
                }

                var result = ProcessFileContents(userName, fileName, fileContents);
                response.SaveResults.Add(result);
                if (result.ValidationResults.Any())
                {
                    _EmailProcessorService.ProcessAndSendValidationErrors(fileName, result.ValidationResults, fileContents);

                    response.ValidationErrors.Add(fileName, result.ValidationResults);
                }
            }

            _EmailProcessorService.ProcessAndSendFailedFiles(response.FailedDownloads, inboundFtpPath);

            return response;
        }

        /// <summary>
        /// Processes the file content
        /// </summary>
        /// <param name="userName">Username requesting the processing</param>
        /// <param name="fileName">FIlename to be processed</param>
        /// <param name="fileContents">File content as string</param>
        /// <returns>WWTVSaveResult object</returns>
        public WWTVSaveResult ProcessFileContents(string userName, string fileName, string fileContents)
        {
            List<WWTVInboundFileValidationResult> validationErrors = new List<WWTVInboundFileValidationResult>();
            InboundFileSaveRequest saveRequest = ParseWWTVFile(fileName, fileContents, validationErrors);
            if (validationErrors.Any())
            {
                return _PostLogService.SaveKeepingTracValidationErrors(saveRequest, userName, validationErrors);
            }

            WWTVSaveResult result;
            try
            {
                result = _PostLogService.SaveKeepingTracFile(saveRequest, userName, DateTime.Now);
            }
            catch (Exception e)
            {
                string errorMessage = "Error saving post log file:\n\n" + e.ToString();
                _EmailProcessorService.ProcessAndSendTechError(fileName, errorMessage, fileContents);
                return null;
            }

            return result;
        }

        /// <summary>
        /// Process error files created by WWTV based on files uploaded by us
        /// </summary>
        public void ProcessErrorFiles()
        {
            _WWTVSharedNetworkHelper.Impersonate(delegate
            {
                var files = _WWTVFtpHelper.GetFtpErrorFileList(BroadcastServiceSystemParameter.WWTV_KeepingTracErrorFtpFolder);
                var remoteFTPPath = _WWTVFtpHelper.GetRemoteFullPath(BroadcastServiceSystemParameter.WWTV_KeepingTracErrorFtpFolder);

                var localPaths = DownloadFTPFiles(files, remoteFTPPath);
                EmailFTPErrorFiles(localPaths);
            });
        }        
    }
}
