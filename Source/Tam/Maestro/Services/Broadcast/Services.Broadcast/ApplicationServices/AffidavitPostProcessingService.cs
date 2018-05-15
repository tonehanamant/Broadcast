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

        /// <summary>
        /// Process and WWTV post processing file
        /// </summary>
        /// <param name="filePath">Path of the file to process</param>
        /// <returns>BaseResponse object</returns>
        AffidavitSaveRequest ParseWWTVFile(string filePath,out string errorMessage);

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
            var filesFailedDownload = new List<string>();
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

            List<string> downloadedFiles = new List<string>();
            if (!filesToProcess.Any())
            {
                return;
            }

            using (WWTVSharedNetworkHelper.GetLocalErrorFolderConnection())
            {
                foreach (var file in filesToProcess)
                {
                    // no need to use shared connection for local temp path (unless we have to)
                    string filePath = $"{Path.GetTempPath()}{file}";
                    try
                    {
                        _DownloadFileFromWWTVFtp(file, filePath);
                    }
                    catch (Exception e)
                    {
                        filesFailedDownload.Add(string.Format("{0} :: Reason -> {1}", file, e.Message));
                        continue; // skip to next file 
                    }

                    downloadedFiles.Add(filePath);
                    AffidavitSaveResult response = null;

                    string errorMessage;
                    AffidavitSaveRequest affidavitSaveRequest = ParseWWTVFile(filePath, out errorMessage);
                    if (!string.IsNullOrEmpty(errorMessage))
                    {
                        ProcessErrorWWTVFile(filePath, errorMessage);
                        continue;
                    }

                    try
                    {
                        _AffidavidService.SaveAffidavit(affidavitSaveRequest, userName, DateTime.Now);
                    }
                    catch (Exception e)
                    {
                        errorMessage = "Error saving affidavit:\n\n" + e.ToString();
                        ProcessErrorWWTVFile(filePath, errorMessage);
                        continue;
                    }
                }

                if (filesFailedDownload.Any())
                {
                    _ProcessFailedFiles(filesFailedDownload);
                }

                var outbound = WWTVFtpHelper.GetInboundPath();
                var ftpFilesToDelete = downloadedFiles.Select(d => outbound + "/" + Path.GetFileName(d)).ToList();
                try
                {
                    WWTVFtpHelper.DeleteFiles(ftpFilesToDelete);
                }
                catch (Exception e)
                {
                    var errorMessage = "Error deleting affidavit file(s) from FTP site:\n\n" + e.ToString();
                    ftpFilesToDelete.ForEach(filePath => ProcessErrorWWTVFile(filePath, errorMessage));
                }
            }
        }

        private void _ProceseTotalFTPFailure(Exception e)
        {
            var emailBody =
                "There was an error reading from or connecting to the FTP server. \n\nHere is some technical information." + e;
            _AffidavitEmailSenderService.Send(emailBody);
        }
        private void _ProcessFailedFiles(List<string> filesFailedDownload)
        {
            var emailBody = "The following file(s) could not be downloaded.\n\n";
            foreach (var file in filesFailedDownload)
            {
                emailBody += string.Format("{0}\n",file);

            }
            _AffidavitEmailSenderService.Send(emailBody);
        }

        public void ProcessErrorWWTVFile(string filePath,string errorMessage)
        {
            var invalidFilePath = _MoveFileToInvalidFilesFolder(filePath);

            var emailBody = _CreateInvalidFileEmailBody(errorMessage, invalidFilePath);

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
        public AffidavitSaveRequest ParseWWTVFile(string filePath,out string errorMessage)
        {
            AffidavitSaveRequest affidavitSaveRequest = new AffidavitSaveRequest();
            affidavitSaveRequest.FileName = Path.GetFileName(filePath);
            errorMessage = "";

            try
            {
                if (!File.Exists(filePath))
                {
                    throw new Exception("File does not exist.");
                }

                if (!Path.GetExtension(filePath).Equals(".txt"))
                {
                    throw new Exception("Invalid file extension.");
                }

                _MapWWTVFileToAffidavitFile(affidavitSaveRequest,filePath,out errorMessage);
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                errorMessage = "Could not process file.\n  " + e.ToString();

                return affidavitSaveRequest;
            }

            affidavitSaveRequest.FileHash = HashGenerator.ComputeHash(File.ReadAllBytes(filePath));

            return affidavitSaveRequest;
        }

        private string _CreateInvalidFileEmailBody(string errorMessage,string filePath)
        {
            var emailBody = new StringBuilder();

            emailBody.AppendFormat("File {0} could not be properly processed.  Including technical information to help figure out the issue.", Path.GetFileName(filePath));

            emailBody.AppendFormat("\n\nFile located in {0}\n", filePath);

            emailBody.AppendFormat("\nTechnical Information:\n\n{0}", errorMessage);

            return emailBody.ToString();
        }

        private string _MoveFileToInvalidFilesFolder(string fileName)
        {
            var failFolder = WWTVSharedNetworkHelper.GetLocalErrorFolder();
            var combinedFilePath = Path.Combine(failFolder,Path.GetFileName(fileName));

            if (File.Exists(combinedFilePath))
                File.Delete(combinedFilePath);

            File.Move(fileName, combinedFilePath);

            return combinedFilePath;
        }


        
        /// <summary>
        /// Deals with fact that time comes in 2 formats  HHMMTT 
        /// and HMMTT (single and double digit hour which is not supported properly by .NET library)
        /// </summary>
        private static System.TimeSpan ExtractTimeHacky(string timeToParse, ref string errorMessage, string fieldName, int recordNumber)
        {
            Regex regExp = new Regex(@"^(?<hours>(([0][1-9]|[1][0-2]|[0-9])))(?<minutes>([0-5][0-9]))(?<ampm>A|P)$");
            var match = regExp.Match(timeToParse);

            if (!match.Success)
            {
                errorMessage += $"Record: {recordNumber + 1}: field: '{fieldName}' is invalid time.  Please use format \"HHMMA|P\".\n";
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

        private System.TimeSpan ExtractDateTime(string datetime, ref string errorMessage, string fieldName, int recordNumber)
        {
            if (!DateTime.TryParse(datetime, out DateTime parsedTime))
            {
                errorMessage += $"Record: {recordNumber+1}: field: '{fieldName}' is invalid date or time.\n";
            }

            return parsedTime.TimeOfDay;
        }
        private AffidavitSaveRequest _MapWWTVFileToAffidavitFile(AffidavitSaveRequest affidavitSaveRequest,string filePath,out string errorMessage)
        {
            affidavitSaveRequest.Source = (int) AffidaviteFileSourceEnum.Strata;
            affidavitSaveRequest.Details = new List<AffidavitSaveRequestDetail>();
            errorMessage = "";

            WhosWatchingTVPostProcessingFile jsonFile;
            try
            {
                jsonFile = new WhosWatchingTVPostProcessingFile();
                jsonFile.Details = JsonConvert.DeserializeObject<List<WhosWatchingTVDetail>>(File.ReadAllText(filePath));
            }
            catch (Exception e)
            {
                errorMessage =
                    "File is in an invalid format.  It cannot be read in its current state; must be a valid JSON file." +
                    "\r\n" + e.ToString();
                return affidavitSaveRequest;
            }

            errorMessage = "";

            for (var recordNumber = 0; recordNumber < jsonFile.Details.Count; recordNumber++)
            {
                var jsonDetail = jsonFile.Details[recordNumber];

                var airTime = jsonDetail.Date.Add(ExtractTimeHacky(jsonDetail.Time, ref errorMessage, "Time", recordNumber));
                var leadInEndTime = jsonDetail.Date.Add(ExtractDateTime(jsonDetail.LeadInEndTime, ref errorMessage, "LeadInEndTime", recordNumber));
                var leadOutStartTime = jsonDetail.Date.Add(ExtractDateTime(jsonDetail.LeadOutStartTime, ref errorMessage, "LeadOutStartTime", recordNumber));


                if (!Enum.TryParse(jsonDetail.InventorySource, out AffidaviteFileSourceEnum inventorySource))
                {
                    errorMessage += $"Record: {recordNumber + 1}: field: 'InventorySource' is inventory source.\n";
                }

                if (!string.IsNullOrEmpty(errorMessage))
                    continue;

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

            if (!string.IsNullOrEmpty(errorMessage))
                errorMessage = $"Found some date/time errors within the file.  \r\n\r\n{errorMessage}";
            return affidavitSaveRequest;
        }

        /// <summary>
        /// It is assumed filePath is locally accessible resource and not networked resource
        /// </summary>
        private void _DownloadFileFromWWTVFtp(string fileName, string filePath)
        {
            var shareFolder = WWTVFtpHelper.GetInboundPath();
            using (var ftpClient = new WebClient())
            {
                ftpClient.Credentials = WWTVFtpHelper.GetFtpClientCredentials();
                ftpClient.DownloadFile($"{shareFolder}/{fileName}",
                    filePath);
            }
        }

        private List<string> _GetWWTVFTPFileNames()
        {
            string uri = WWTVFtpHelper.GetInboundPath();
            return WWTVFtpHelper.GetFileList(uri, (file) => file.EndsWith(VALID_INCOMING_FILE_EXTENSION));
        }
    }
}