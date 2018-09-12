﻿using Common.Services.ApplicationServices;
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
    }

    public class PostLogPostProcessingService : BasePostProcessingService, IPostLogPostProcessingService
    {
        private readonly IWWTVFtpHelper _WWTVFtpHelper;
        private readonly IWWTVEmailProcessorService _EmailProcessorService;
        private readonly IPostLogService _PostLogService;

        public PostLogPostProcessingService(IWWTVFtpHelper wwtvFtpHelper
            , IWWTVEmailProcessorService emailService
            , IPostLogService postLogService
            , IFileTransferEmailHelper emailHelper
            , IBroadcastAudiencesCache audienceCache) : base(emailHelper, wwtvFtpHelper, audienceCache)
        {
            _WWTVFtpHelper = wwtvFtpHelper;
            _EmailProcessorService = emailService;
            _PostLogService = postLogService;
        }

        /// <summary>
        /// Downloads the WWTV processed files and calls the postlog processing service
        /// return true if download success, false if download fails (use for loggin)
        /// This involves FTP 
        /// </summary>
        public DownloadAndProcessWWTVFilesResponse DownloadAndProcessWWTVFiles(string userName)
        {
            List<string> filesToProcess = DownloadFilesToBeProcessed(BroadcastServiceSystemParameter.WWTV_KeepingTracFtpInboundFolder);
            var response = new DownloadAndProcessWWTVFilesResponse();
            if (!filesToProcess.Any())
            {
                return response;
            }
            response.FilesFoundToProcess.AddRange(filesToProcess);

            var inboundFtpPath = _WWTVFtpHelper.GetRemoteFullPath(BroadcastServiceSystemParameter.WWTV_KeepingTracFtpInboundFolder);

            foreach (var filePath in filesToProcess)
            {
                //download file content
                string fileContents = _WWTVFtpHelper.DownloadFTPFileContent(filePath, out bool success, out string errorMessage);
                if (!success)
                {
                    response.FailedDownloads.Add(filePath + " Reason: " + errorMessage);
                    continue;
                }

                string fileName = Path.GetFileName(filePath);


                var ftpFileToDelete = inboundFtpPath + "/" + fileName;
                try
                {
                    _WWTVFtpHelper.DeleteFiles(ftpFileToDelete);
                }
                catch (Exception e)
                {
                    var errorDeletingFile = "Error deleting post log file from FTP site: " + ftpFileToDelete + "\r\n" + e;
                    _EmailProcessorService.ProcessAndSendTechError(filePath, errorMessage, fileContents);
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
                result = _PostLogService.SaveKeepingTrac(saveRequest, userName, DateTime.Now);
            }
            catch (Exception e)
            {
                string errorMessage = "Error saving post log file:\n\n" + e.ToString();
                _EmailProcessorService.ProcessAndSendTechError(fileName, errorMessage, fileContents);
                return null;
            }

            return result;
        }
    }
}
