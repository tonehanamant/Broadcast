using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Services.Broadcast.Helpers;
using Services.Broadcast.ApplicationServices.Helpers;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace Services.Broadcast.ApplicationServices
{
    public interface IAffidavitPostProcessingService : IApplicationService
    {
        /// <summary>
        /// Downloads the WWTV processed files and calls the affidavit processing service
        /// </summary>
        DownloadAndProcessWWTVFilesResponse DownloadAndProcessWWTVFiles(string userName);

        WWTVSaveResult ProcessFileContents(string userName, string fileName, string fileContents);
    }


    public class AffidavitPostProcessingService : BasePostProcessingService, IAffidavitPostProcessingService
    {
        private readonly IWWTVEmailProcessorService _affidavitEmailProcessorService;
        private readonly IAffidavitService _AffidavidService;
        private readonly IWWTVFtpHelper _WWTVFtpHelper;

        private const string VALID_INCOMING_FILE_EXTENSION = ".txt";

        public AffidavitPostProcessingService(
            IDataRepositoryFactory broadcastDataRepositoryFactory
            , IWWTVEmailProcessorService affidavitEmailProcessorService
            , IAffidavitService affidavidService
            , IWWTVFtpHelper ftpHelper
            , IFileTransferEmailHelper emailHelper
            , IBroadcastAudiencesCache audienceCache) : base(emailHelper, ftpHelper, audienceCache)
        {
            _affidavitEmailProcessorService = affidavitEmailProcessorService;
            _AffidavidService = affidavidService;
            _WWTVFtpHelper = ftpHelper;
        }
        
        /// <summary>
        /// Downloads the WWTV processed files and calls the affidavit processing service
        /// return true if download success, false if download fails (use for loggin)
        /// This involves FTP 
        /// </summary>
        public DownloadAndProcessWWTVFilesResponse DownloadAndProcessWWTVFiles(string userName)
        {
            List<string> filesToProcess = DownloadFilesToBeProcessed(BroadcastServiceSystemParameter.WWTV_FtpInboundFolder);
            var response = new DownloadAndProcessWWTVFilesResponse();            
            if (!filesToProcess.Any())
            {
                return response;
            }
            response.FilesFoundToProcess.AddRange(filesToProcess);

            var inboundFtpPath = _WWTVFtpHelper.GetRemoteFullPath(BroadcastServiceSystemParameter.WWTV_FtpInboundFolder);

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
                    var errorDeletingFile = "Error deleting affidavit file from FTP site: " + ftpFileToDelete + "\r\n" + e;
                    _affidavitEmailProcessorService.ProcessAndSendTechError(filePath, errorDeletingFile, fileContents);
                    continue;
                }

                var result = ProcessFileContents(userName, fileName, fileContents);
                response.SaveResults.Add(result);
                if (result.ValidationResults.Any())
                {
                    _affidavitEmailProcessorService.ProcessAndSendValidationErrors(fileName, result.ValidationResults,
                        fileContents);

                    response.ValidationErrors.Add(fileName, result.ValidationResults);
                }
            }

            _affidavitEmailProcessorService.ProcessAndSendFailedFiles(response.FailedDownloads, inboundFtpPath);

            return response;
        }

        public WWTVSaveResult ProcessFileContents(string userName, string fileName, string fileContents)
        {
            List<WWTVInboundFileValidationResult> validationErrors = new List<WWTVInboundFileValidationResult>();
            InboundFileSaveRequest affidavitSaveRequest = ParseWWTVFile(fileName, fileContents, validationErrors);
            if (validationErrors.Any())
            {
                return _AffidavidService.SaveAffidavitValidationErrors(affidavitSaveRequest, userName, validationErrors);
            }

            WWTVSaveResult result;
            try
            {
                result = _AffidavidService.SaveAffidavit(affidavitSaveRequest, userName, DateTime.Now);
            }
            catch (Exception e)
            {
                string errorMessage = "Error saving affidavit:\n\n" + e.ToString();
                _affidavitEmailProcessorService.ProcessAndSendTechError(fileName, errorMessage,fileContents);
                return null;
            }

            return result;
        }
    }
}