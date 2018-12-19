using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Common.Services;
using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.ApplicationServices.Helpers;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using Tam.Maestro.Common;
using Tam.Maestro.Services.ContractInterfaces;

namespace Services.Broadcast.BusinessEngines
{
    public interface IWWTVEmailProcessorService : IApplicationService
    {
        int ProcessAndSendValidationErrors(string filePath, List<WWTVInboundFileValidationResult> validationErrors, string fileContents);
        int ProcessAndSendTechError(string filePath, string errorMessage, string fileContents);
        void ProcessAndSendFailedFiles(List<string> filesFailedDownload, string ftpLocation);
        void ProcessAndSendInvalidDataFiles(List<WWTVOutboundFileValidationResult> validationList);
        string CreateValidationErrorEmailBody(List<WWTVInboundFileValidationResult> validationErrors, string fileName);
        string CreateTechErrorEmailBody(string errorMessage, string filePath);
        string CreateFailedFTPFileEmailBody(List<string> filesFailedDownload, string ftpLocation);

        /// <summary>
        /// Logs any errors that happened in DownloadAndProcessWWTV Files and ParseWWTVFile.
        /// Do not call directly, only used for Integration testing
        /// </summary>
        int LogAffidavitError(string filePath, string errorMessage);

    }

    public class WWTVEmailProcessorService : IWWTVEmailProcessorService
    {
        private const string _EmailValidationSubject = "WWTV File Failed Validation";
        private readonly IAffidavitRepository _AffidavitRepository;
        private readonly IEmailerService _EmailerService;
        private readonly IFileTransferEmailHelper _EmailHelper;
        private readonly IFileService _FileService;

        public WWTVEmailProcessorService(IEmailerService emailerService,
                                                IDataRepositoryFactory broadcastDataRepositoryFactory,
                                                IFileTransferEmailHelper emailHelper,
                                                IFileService fileService)
        {
            _EmailerService = emailerService;
            _EmailHelper = emailHelper;
            _AffidavitRepository = broadcastDataRepositoryFactory.GetDataRepository<IAffidavitRepository>();
            _FileService = fileService;
        }

        private void _SaveFileContentsToErrorFolder(string fileName, string fileContents)
        {
            var fullPath = WWTVSharedNetworkHelper.BuildLocalErrorPath(fileName);
            File.WriteAllText(fullPath, fileContents);
        }
        public int ProcessAndSendValidationErrors(string filePath, List<WWTVInboundFileValidationResult> validationErrors, string fileContents)
        {
            _SaveFileContentsToErrorFolder(Path.GetFileName(filePath), fileContents);
            if (validationErrors == null || !validationErrors.Any())
                return -1;

            var emailBody = CreateValidationErrorEmailBody(validationErrors, filePath);
            _EmailHelper.SendEmail(emailBody, _EmailValidationSubject);
            return LogAffidavitError(filePath, emailBody);

        }

        public int ProcessAndSendTechError(string filePath, string errorMessage, string fileContents)
        {
            _SaveFileContentsToErrorFolder(Path.GetFileName(filePath), fileContents);
            var emailBody = CreateTechErrorEmailBody(errorMessage, filePath);

            _EmailHelper.SendEmail(emailBody, "WWTV File Failed");

            return LogAffidavitError(filePath, errorMessage);
        }

        public void ProcessAndSendFailedFiles(List<string> filesFailedDownload, string ftpLocation)
        {
            if (!filesFailedDownload.Any())
                return;

            var emailBody = CreateFailedFTPFileEmailBody(filesFailedDownload, ftpLocation);
            _EmailHelper.SendEmail(emailBody, "WWTV File Failed");
        }


        /// <summary>
        /// Move invalid files to invalid files folder. Notify users about failed files
        /// </summary>
        /// <param name="files">List of OutboundAffidavitFileValidationResultDto objects representing the invalid files to process</param>
        public void ProcessAndSendInvalidDataFiles(List<WWTVOutboundFileValidationResult> invalidFiles)
        {
            foreach (var invalidFile in invalidFiles)
            {
                var invalidFilePath = _FileService.Move(invalidFile.FilePath, WWTVSharedNetworkHelper.GetLocalErrorFolder());

                var emailBody = _EmailHelper.CreateInvalidDataFileEmailBody(invalidFile.ErrorMessages, invalidFilePath, invalidFile.FileName);

                _EmailHelper.SendEmail(emailBody, "Error Preprocessing");
            }
        }
        
        public string CreateFailedFTPFileEmailBody(List<string> filesFailedDownload, string ftpLocation)
        {
            var emailBody = "The following file(s) could not be downloaded from:\r\n" + ftpLocation + "\n\n";
            foreach (var file in filesFailedDownload)
            {
                emailBody += string.Format("{0}\n", file);
            }

            return emailBody;
        }

        public string CreateValidationErrorEmailBody(List<WWTVInboundFileValidationResult> validationErrors, string fileName)
        {
            var errorMessage = WWTVInboundFileValidationResult.FormatValidationMessage(validationErrors);

            var emailBody = new StringBuilder();

            emailBody.AppendFormat("File {0} file validation for WWTV upload.", Path.GetFileName(fileName));

            emailBody.AppendFormat("\n\n{0}", errorMessage);

            var fullPath = string.Format("{0}\\{1}", WWTVSharedNetworkHelper.GetLocalErrorFolder(), fileName);
            emailBody.AppendFormat("\n\nFile located in {0}\n", fullPath);

            return emailBody.ToString();
        }

        public string CreateTechErrorEmailBody(string errorMessage, string fileName)
        {
            var emailBody = new StringBuilder();

            emailBody.AppendFormat("File {0} could not be properly processed.  Including technical information to help figure out the issue.", Path.GetFileName(fileName));

            var fullPath = string.Format("{0}\\{1}", WWTVSharedNetworkHelper.GetLocalErrorFolder(), fileName);
            emailBody.AppendFormat("\n\nFile located in {0}\n", fullPath);

            emailBody.AppendFormat("\nTechnical Information:\n\n{0}", errorMessage);

            return emailBody.ToString();
        }

        public int LogAffidavitError(string filePath, string errorMessage)
        {
            var affidavitFile = new ScrubbingFile
            {
                FileName = Path.GetFileName(filePath),
                Status = FileProcessingStatusEnum.Invalid,
                FileHash = HashGenerator.ComputeHash(filePath.ToByteArray()), // just so there is something
                CreatedDate = DateTime.Now,
                SourceId = (int)AffidavitFileSourceEnum.Strata
            };

            affidavitFile.FileProblems.Add(new FileProblem
            {
                ProblemDescription = errorMessage
            });
            return _AffidavitRepository.SaveAffidavitFile(affidavitFile);
        }
    }
}
