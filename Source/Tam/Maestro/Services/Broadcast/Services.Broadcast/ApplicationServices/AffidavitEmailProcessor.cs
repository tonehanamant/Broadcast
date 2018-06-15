using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using Tam.Maestro.Common;
using Tam.Maestro.Services.Cable.SystemComponentParameters;
using Tam.Maestro.Services.ContractInterfaces;

namespace Services.Broadcast.BusinessEngines
{
    public interface IAffidavitEmailProcessorService : IApplicationService
    {
        int ProcessAndSendValidationErrors(string filePath, List<AffidavitValidationResult> validationErrors,string fileContents);
        int ProcessAndSendTechError(string filePath, string errorMessage, string fileContents);
        void ProcessAndSendFailedFiles(List<string> filesFailedDownload, string ftpLocation);
        void ProcessAndSendInvalidDataFiles(List<OutboundAffidavitFileValidationResultDto> validationList);
        string CreateValidationErrorEmailBody(List<AffidavitValidationResult> validationErrors, string fileName);
        string CreateTechErrorEmailBody(string errorMessage, string filePath);
        string CreateFailedFTPFileEmailBody(List<string> filesFailedDownload, string ftpLocation);
        string CreateInvalidDataFileEmailBody(OutboundAffidavitFileValidationResultDto invalidFile,string invalidFilePath);
        void Send(string emailBody,string subject);

        /// <summary>
        /// Logs any errors that happened in DownloadAndProcessWWTV Files and ParseWWTVFile.
        /// Do not call directly, only used for Integration testing
        /// </summary>
        int LogAffidavitError(string filePath, string errorMessage);

    }

    public class AffidavitEmailProcessorService :  IAffidavitEmailProcessorService
    {
        private const string _EmailValidationSubject = "WWTV File Failed Validation";
        private readonly IAffidavitRepository _AffidavitRepository;

        public AffidavitEmailProcessorService(IDataRepositoryFactory broadcastDataRepositoryFactory)
        {
            _AffidavitRepository = broadcastDataRepositoryFactory.GetDataRepository<IAffidavitRepository>();
        }

        private void _SaveFileContentsToErrorFolder(string fileName, string fileContents)
        {
            var fullPath = WWTVSharedNetworkHelper.BuildLocalErrorPath(fileName);
            File.WriteAllText(fullPath, fileContents);
        }
        public int ProcessAndSendValidationErrors(string filePath, List<AffidavitValidationResult> validationErrors,string fileContents)
        {
            _SaveFileContentsToErrorFolder(Path.GetFileName(filePath), fileContents);
            if (validationErrors == null || !validationErrors.Any())
                return -1;

            var emailBody = CreateValidationErrorEmailBody(validationErrors, filePath);
            Send(emailBody, _EmailValidationSubject);
            return LogAffidavitError(filePath, emailBody);

        }

        public int ProcessAndSendTechError(string filePath, string errorMessage,string fileContents)
        {
            _SaveFileContentsToErrorFolder(Path.GetFileName(filePath),fileContents);
            var emailBody = CreateTechErrorEmailBody(errorMessage, filePath);

            Send(emailBody, "WWTV File Failed");

            return LogAffidavitError(filePath, errorMessage);
        }

        public void ProcessAndSendFailedFiles(List<string> filesFailedDownload, string ftpLocation)
        {
            if (!filesFailedDownload.Any())
                return;

            var emailBody = CreateFailedFTPFileEmailBody(filesFailedDownload, ftpLocation);
            Send(emailBody, "WWTV File Failed");
        }


        /// <summary>
        /// Move invalid files to invalid files folder. Notify users about failed files
        /// </summary>
        /// <param name="files">List of OutboundAffidavitFileValidationResultDto objects representing the valid files to be sent</param>
        public void ProcessAndSendInvalidDataFiles(List<OutboundAffidavitFileValidationResultDto> validationList)
        {
            var invalidFiles = validationList.Where(v => v.Status == AffidaviteFileProcessingStatus.Invalid);

            foreach (var invalidFile in invalidFiles)
            {
                var invalidFilePath = _MoveInvalidFileToArchiveFolder(invalidFile);

                var emailBody = CreateInvalidDataFileEmailBody(invalidFile, invalidFilePath);

                Send(emailBody, "Error Preprocessing");
            }
        }

        private string _MoveInvalidFileToArchiveFolder(OutboundAffidavitFileValidationResultDto invalidFile)
        {
            var combinedFilePath = WWTVSharedNetworkHelper.BuildLocalErrorPath(Path.GetFileName(invalidFile.FilePath));

            if (File.Exists(combinedFilePath))
                File.Delete(combinedFilePath);

            File.Move(invalidFile.FilePath, combinedFilePath);

            return combinedFilePath;
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

        public string CreateValidationErrorEmailBody(List<AffidavitValidationResult> validationErrors, string fileName)
        {
            var errorMessage = AffidavitValidationResult.FormatValidationMessage(validationErrors);

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

        public string CreateInvalidDataFileEmailBody(OutboundAffidavitFileValidationResultDto invalidFile, string invalidFilePath)
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


        public int LogAffidavitError(string filePath, string errorMessage)
        {
            var affidavitFile = new AffidavitFile();
            affidavitFile.FileName = Path.GetFileName(filePath);
            affidavitFile.Status = AffidaviteFileProcessingStatus.Invalid;
            affidavitFile.FileHash = HashGenerator.ComputeHash(filePath.ToByteArray()); // just so there is something
            affidavitFile.CreatedDate = DateTime.Now;
            affidavitFile.SourceId = (int)AffidaviteFileSourceEnum.Strata;

            var problem = new AffidavitFileProblem();
            problem.ProblemDescription = errorMessage;

            affidavitFile.AffidavitFileProblems.Add(problem);
            var id = _AffidavitRepository.SaveAffidavitFile(affidavitFile);

            return id;
        }




        public void Send(string emailBody, string subject )
        {
            if (!BroadcastServiceSystemParameter.EmailNotificationsEnabled)
                return;

            var from = new MailAddress(BroadcastServiceSystemParameter.EmailUsername);
            var to = new List<MailAddress>() {new MailAddress(BroadcastServiceSystemParameter.WWTV_NotificationEmail)};
            Emailer.QuickSend(false,emailBody, subject,MailPriority.Normal,from ,to);
        }
    }
}
