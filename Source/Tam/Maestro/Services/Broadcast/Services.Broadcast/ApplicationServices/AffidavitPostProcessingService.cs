using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Newtonsoft.Json;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Repositories;
using Tam.Maestro.Common;
using Tam.Maestro.Services.Cable.Entities;
using Tam.Maestro.Services.Cable.SystemComponentParameters;
using Tam.Maestro.Services.ContractInterfaces;
using TimeSpan = Tam.Maestro.Services.ContractInterfaces.InventoryBusinessObjects.TimeSpan;

namespace Services.Broadcast.ApplicationServices
{
    public interface IAffidavitPostProcessingService : IApplicationService
    {
        /// <summary>
        /// Downloads the WWTV processed files and calls the affidavit processing service
        /// </summary>
        void DownloadAndProcessWWTVFiles(string userName);

        AffidavitSaveResult ProcessFileContents(string userName, string fileName, string fileContents);

        /// <summary>
        /// Logs any errors that happened in DownloadAndProcessWWTV Files and ParseWWTVFile.
        /// Do not call directly, only used for Integration testing
        /// </summary>
        int LogAffidavitError(string filePath, string errorMessage);
    }


    public class AffidavitPostProcessingService : IAffidavitPostProcessingService
    {
        private readonly IBroadcastAudiencesCache _AudienceCache;
        private readonly IAffidavitEmailSenderService _AffidavitEmailSenderService;
        private readonly IAffidavitService _AffidavidService;
        private readonly IAffidavitRepository _AffidavitRepository;

        private const string VALID_INCOMING_FILE_EXTENSION = ".txt";
        private const string HTTP_ACCEPT_HEADER = "application/json";

        private const string _EmailValidationSubject = "WWTV File Failed Validation";

        public AffidavitPostProcessingService(
            IBroadcastAudiencesCache audienceCache,
            IDataRepositoryFactory broadcastDataRepositoryFactory,
            IAffidavitEmailSenderService affidavitEmailSenderService,
            IAffidavitService affidavidService)
        {
            _AudienceCache = audienceCache;
            _AffidavitEmailSenderService = affidavitEmailSenderService;
            _AffidavidService = affidavidService;
            _AffidavitRepository = broadcastDataRepositoryFactory.GetDataRepository<IAffidavitRepository>();
        }



        /// <summary>
        /// Downloads the WWTV processed files and calls the affidavit processing service
        /// return true if download success, false if download fails (use for loggin)
        /// This involves FTP 
        /// </summary>
        public void DownloadAndProcessWWTVFiles(string userName)
        {
            List<string> filesToProcess;
            try
            {
                filesToProcess = _GetWWTVFTPFileNames();
            }
            catch (Exception e)
            {
                _ProceseTotalFTPFailure(e);
                throw;
            }

            if (!filesToProcess.Any())
            {
                return;
            }

            var inboundFtpPath = WWTVFtpHelper.GetFTPInboundPath();

            var failedDownloads = new List<string>();
            foreach (var filePath in filesToProcess)
            {
                string fileContents;
                try
                {
                    fileContents = _DownloadFileFromWWTVFtpToString(filePath);
                }
                catch (Exception e)
                {
                    failedDownloads.Add(filePath + " Reason: " + e);
                    continue; // skip to next file 
                }
                string fileName = Path.GetFileName(filePath);

                var ftpFileToDelete = inboundFtpPath + "/" + fileName;
                try
                {
                    WWTVFtpHelper.DeleteFile(ftpFileToDelete);
                }
                catch (Exception e)
                {
                    var errorMessage = "Error deleting affidavit file from FTP site: " + ftpFileToDelete + "\r\n" + e;
                    _ProcessTechErrorWWTVFile(filePath, errorMessage);
                    continue;
                }

                var result = ProcessFileContents(userName, fileName, fileContents);

                if (result.ValidationResults.Any())
                    _ProcessValidationErrors(fileName, result.ValidationResults);
            }

            _ProcessFailedFiles(failedDownloads,inboundFtpPath);
        }

        public AffidavitSaveResult ProcessFileContents(string userName, string fileName, string fileContents)
        {
            List<AffidavitValidationResult> validationErrors = new List<AffidavitValidationResult>();
            AffidavitSaveRequest affidavitSaveRequest = ParseWWTVFile(fileName, fileContents, validationErrors);
            if (validationErrors.Any())
            {
                
                return new AffidavitSaveResult{ ValidationResults = validationErrors };
            }

            AffidavitSaveResult result;
            try
            {
                result = _AffidavidService.SaveAffidavit(affidavitSaveRequest, userName, DateTime.Now);
            }
            catch (Exception e)
            {
                string errorMessage = "Error saving affidavit:\n\n" + e.ToString();
                _ProcessTechErrorWWTVFile(fileName, errorMessage);
                return null;
            }

            return result;
        }


        private void _ProcessValidationErrors(string fileName, List<AffidavitValidationResult> validationErrors)
        {
            string message = AffidavitValidationResult.FormatValidationMessage(validationErrors);

            if (string.IsNullOrEmpty(message))
                return;

            var emailBody = _CreateValidationErrorEmailBody(message, fileName);
            _AffidavitEmailSenderService.Send(emailBody, _EmailValidationSubject);
        }
    
        private void _ProceseTotalFTPFailure(Exception e)
        {
            var emailBody =
                "There was an error reading from or connecting to the FTP server. \n\nHere is some technical information." + e;
            _AffidavitEmailSenderService.Send(emailBody,"WWTV FTP Error");
        }
        private void _ProcessFailedFiles(List<string> filesFailedDownload,string ftpLocation)
        {
            if (!filesFailedDownload.Any())
                return;

            var emailBody = "The following file(s) could not be downloaded from:\r\n" + ftpLocation;
            foreach (var file in filesFailedDownload)
            {
                emailBody += string.Format("{0}\n",file);

            }
            _AffidavitEmailSenderService.Send(emailBody, "WWTV File Failed");
        }

        private void _ProcessTechErrorWWTVFile(string filePath,string errorMessage)
        {
            var emailBody = _CreateTechErrorEmailBody(errorMessage, filePath);

            _AffidavitEmailSenderService.Send(emailBody, "WWTV File Failed");

            LogAffidavitError(filePath,errorMessage);
        }

        public int LogAffidavitError(string filePath,string errorMessage)
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

        /// <summary>
        /// Process and WWTV post processing file
        /// </summary>
        /// <param name="filePath">Path of the file to process</param>
        /// <returns>BaseResponse object containing the new id of the affidavit_file</returns>
        public AffidavitSaveRequest ParseWWTVFile(string fileName,string fileContents, List<AffidavitValidationResult> validationErrors)
        {
            AffidavitSaveRequest affidavitSaveRequest = new AffidavitSaveRequest();
            try
            {
                if (!Path.GetExtension(fileName).Equals(VALID_INCOMING_FILE_EXTENSION))
                {
                    throw new Exception("Invalid file extension.");
                }
                affidavitSaveRequest.FileName = Path.GetFileName(fileName);

                _MapWWTVFileToAffidavitFile(fileContents,affidavitSaveRequest, validationErrors);
            }
            catch (Exception e)
            {
                validationErrors.Add(new AffidavitValidationResult()
                {
                    ErrorMessage = "Could not process file.\n  " + e.ToString()
                });

                return affidavitSaveRequest;
            }

            affidavitSaveRequest.FileHash = HashGenerator.ComputeHash(fileContents.ToByteArray());

            return affidavitSaveRequest;
        }
        private string _CreateValidationErrorEmailBody(string errorMessage, string filePath)
        {
            var emailBody = new StringBuilder();

            emailBody.AppendFormat("File {0} file validation for WWTV upload.", Path.GetFileName(filePath));

            emailBody.AppendFormat("\n\n{0}", errorMessage);

            emailBody.AppendFormat("\n\nFile located in {0}\n", filePath);

            return emailBody.ToString();
        }

        private string _CreateTechErrorEmailBody(string errorMessage,string filePath)
        {
            var emailBody = new StringBuilder();

            emailBody.AppendFormat("File {0} could not be properly processed.  Including technical information to help figure out the issue.", Path.GetFileName(filePath));

            emailBody.AppendFormat("\n\nFile located in {0}\n", filePath);

            emailBody.AppendFormat("\nTechnical Information:\n\n{0}", errorMessage);

            return emailBody.ToString();
        }

        
        /// <summary>
        /// Deals with fact that time comes in 2 formats  HHMMTT and HMMTT 
        /// (single and double digit hour which is not supported properly by .NET library)
        /// </summary>
        private static System.TimeSpan ExtractTimeHacky(string timeToParse, List<AffidavitValidationResult> validationErrors, string fieldName, int recordNumber)
        {
            Regex regExp = new Regex(@"^(?<hours>(([0][1-9]|[1][0-2]|[0-9])))(?<minutes>([0-5][0-9]))(?<ampm>A|P)$");
            var match = regExp.Match(timeToParse);

            if (!match.Success)
            {
                validationErrors.Add(new AffidavitValidationResult()
                {
                    ErrorMessage = "is invalid time.  Please use format \"HHMMA|P\".",
                    InvalidField = fieldName,
                    InvalidLine = recordNumber
                });
                return new System.TimeSpan();
            }

            DateTime result = new DateTime();

            int hour = Int32.Parse(match.Groups["hours"].Value);
            if (match.Groups["ampm"].Value == "P" && hour < 12)
                hour += 12;
            int minutes = Int32.Parse(match.Groups["minutes"].Value);
            

            result = new DateTime(1,1,1,hour,minutes,0);
            return result.TimeOfDay;
        }

        private System.TimeSpan ExtractDateTime(string datetime, List<AffidavitValidationResult> validationErrors, string fieldName, int recordNumber)
        {
            if (!DateTime.TryParse(datetime, out DateTime parsedTime))
            {
                validationErrors.Add(new AffidavitValidationResult()
                {
                    ErrorMessage = "is invalid date or time.",
                    InvalidField = fieldName,
                    InvalidLine = recordNumber
                });
                return new System.TimeSpan();
            }

            return parsedTime.TimeOfDay;
        }
        private AffidavitSaveRequest _MapWWTVFileToAffidavitFile(string fileContents,  AffidavitSaveRequest affidavitSaveRequest, List<AffidavitValidationResult> validationErrors)
        {
            affidavitSaveRequest.Source = (int) AffidaviteFileSourceEnum.Strata;
            affidavitSaveRequest.Details = new List<AffidavitSaveRequestDetail>();

            WhosWatchingTVPostProcessingFile jsonFile;
            try
            {
                jsonFile = new WhosWatchingTVPostProcessingFile();
                jsonFile.Details = JsonConvert.DeserializeObject<List<WhosWatchingTVDetail>>(fileContents);
            }
            catch (Exception e)
            {
                validationErrors.Add(new AffidavitValidationResult()
                {
                    ErrorMessage = "File is in an invalid format.  It cannot be read in its current state; must be a valid JSON file." +
                                   "\r\n" + e.ToString()
                });
                return affidavitSaveRequest;
            }

            for (var recordNumber = 0; recordNumber < jsonFile.Details.Count; recordNumber++)
            {
                var jsonDetail = jsonFile.Details[recordNumber];

                var airTime = jsonDetail.Date.Add(ExtractTimeHacky(jsonDetail.Time, validationErrors, "Time", recordNumber));
                var leadInEndTime = jsonDetail.Date.Add(ExtractDateTime(jsonDetail.LeadInEndTime, validationErrors, "LeadInEndTime", recordNumber));
                var leadOutStartTime = jsonDetail.Date.Add(ExtractDateTime(jsonDetail.LeadOutStartTime, validationErrors, "LeadOutStartTime", recordNumber));

                if (!Enum.TryParse(jsonDetail.InventorySource, out AffidaviteFileSourceEnum inventorySource))
                {
                    validationErrors.Add(new AffidavitValidationResult()
                    {
                        ErrorMessage = "is invalid",
                        InvalidField = "InventorySource",
                        InvalidLine = recordNumber
                    });
                }

                var affidavitSaveRequestDetail = new AffidavitSaveRequestDetail()
                {
                    Genre = jsonDetail.Genre,
                    AirTime = airTime,
                    Isci = jsonDetail.ISCI,
                    LeadInGenre = jsonDetail.LeadInGenre,
                    LeadInProgramName = jsonDetail.LeadInProgram,
                    LeadOutGenre = jsonDetail.LeadOutGenre,
                    LeadOutProgramName = jsonDetail.LeadOutProgram,
                    Market = jsonDetail.Market,
                    ProgramName = jsonDetail.Program,
                    SpotLength = jsonDetail.SpotLength,
                    Station = jsonDetail.Station,
                    Affiliate = jsonDetail.Affiliate,
                    EstimateId = jsonDetail.EstimateId,
                    InventorySource = inventorySource,
                    SpotCost = jsonDetail.SpotCost,
                    LeadInEndTime = leadInEndTime,
                    LeadOutStartTime = leadOutStartTime,
                    ShowType = jsonDetail.ShowType,
                    LeadInShowType = jsonDetail.LeadInShowType,
                    LeadOutShowType = jsonDetail.LeadOutShowType
                };

                if (jsonDetail.Demographics != null)
                {
                    affidavitSaveRequestDetail.Demographics = jsonDetail.Demographics.Select(y => new AffidavitDemographics()
                    {
                        AudienceId = _AudienceCache.GetDisplayAudienceByCode(y.Demographic).Id,
                        OvernightImpressions = y.OvernightImpressions,
                        OvernightRating = y.OvernightRating
                    }).ToList();

                }

                affidavitSaveRequest.Details.Add(affidavitSaveRequestDetail);
            }
            return affidavitSaveRequest;
        }

        private string _DownloadFileFromWWTVFtpToString(string fileName)
        {
            var shareFolder = WWTVFtpHelper.GetFTPInboundPath();
            using (var ftpClient = new WebClient())
            {
                ftpClient.Credentials = WWTVFtpHelper.GetFtpClientCredentials();
                StreamReader reader = new StreamReader(ftpClient.OpenRead($"{shareFolder}/{fileName}"));
                return reader.ReadToEnd();
            }
        }

        private List<string> _GetWWTVFTPFileNames()
        {
            string uri = WWTVFtpHelper.GetFTPInboundPath();
            return WWTVFtpHelper.GetFileList(uri, (file) => file.EndsWith(VALID_INCOMING_FILE_EXTENSION));
        }
    }
}