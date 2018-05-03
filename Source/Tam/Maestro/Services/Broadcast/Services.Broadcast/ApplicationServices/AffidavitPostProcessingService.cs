using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Newtonsoft.Json;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Mvc;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Repositories;
using Tam.Maestro.Common;
using Tam.Maestro.Services.Cable.Entities;
using Tam.Maestro.Services.Cable.SystemComponentParameters;
using Tam.Maestro.Services.ContractInterfaces;

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
        private const string FTP_SCHEME = "ftp://";

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

            foreach (var file in filesToProcess)
            {
                string filePath = $"{Path.GetTempPath()}{file}";
                try
                {
                    _DownloadFileFromWWTVFtp(file, filePath);
                }
                catch (Exception e)
                {               
                    // cannot download file
                    filesFailedDownload.Add(string.Format("{0} :: Reason -> {1}",file,e.Message));
                    continue;   // skip to next file 
                }
                AffidavitSaveResult response = null;

                string errorMessage;
                AffidavitSaveRequest affidavitSaveRequest = ParseWWTVFile(filePath,out errorMessage);
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

                try
                {
                    _DeleteWWTVFTPFile(Path.GetFileName(filePath));
                }
                catch (Exception e)
                {
                    errorMessage = "Error deleting affidavit file from FTP site:\n\n" + e.ToString();
                    ProcessErrorWWTVFile(filePath,errorMessage,false);
                    continue;
                }
            }

            if (filesFailedDownload.Any())
            {
                _ProcessFailedFiles(filesFailedDownload);
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

        public void ProcessErrorWWTVFile(string filePath,string errorMessage,bool deleteFtpFile = true)
        {
            var invalidFilePath = _MoveFileToInvalidFilesFolder(filePath);

            var emailBody = _CreateInvalidFileEmailBody(errorMessage, invalidFilePath);

            _AffidavitEmailSenderService.Send(emailBody, "WWTV File Failed");

            if (deleteFtpFile)
                _DeleteWWTVFTPFile(Path.GetFileName(filePath));

            LogAffidavitError(filePath,errorMessage);
        }

        public int LogAffidavitError(string filePath,string errorMessage)
        {
            var affidavitFile = new AffidavitFile();
            affidavitFile.FileName = Path.GetFileName(filePath);
            affidavitFile.Status = AffidaviteFileProcessingStatus.Invalid;
            affidavitFile.FileHash = HashGenerator.ComputeHash(filePath.ToByteArray()); // just so there is something
            affidavitFile.CreatedDate = DateTime.Now;
            affidavitFile.SourceId = (int)AffidaviteFileSource.Strata;

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
                    return affidavitSaveRequest;
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
            var combinedFilePath = Path.Combine(BroadcastServiceSystemParameter.WWTV_FailedFolder,
                Path.GetFileName(fileName));

            if (File.Exists(combinedFilePath))
                File.Delete(combinedFilePath);

            File.Move(fileName, combinedFilePath);

            return combinedFilePath;
        }

        private void _DeleteWWTVFTPFile(string fileName)
        {
            string uri =
                $"{FTP_SCHEME}{BroadcastServiceSystemParameter.WWTV_FtpHost}/{BroadcastServiceSystemParameter.WWTV_FtpInboundFolder}/{fileName}";
            FtpWebRequest request = (FtpWebRequest) WebRequest.Create(uri);
            request.Credentials = new NetworkCredential(BroadcastServiceSystemParameter.WWTV_FtpUsername,
                BroadcastServiceSystemParameter.WWTV_FtpPassword);
            request.Method = WebRequestMethods.Ftp.DeleteFile;

            FtpWebResponse response = (FtpWebResponse) request.GetResponse();
            response.Close();
        }

        private AffidavitSaveRequest _MapWWTVFileToAffidavitFile(AffidavitSaveRequest affidavitSaveRequest,string filePath,out string errorMessage)
        {
            affidavitSaveRequest.Source = (int) AffidaviteFileSource.Strata;
            affidavitSaveRequest.Details = new List<AffidavitSaveRequestDetail>();
            errorMessage = "";

            WhosWatchingTVPostProcessingFile jsonFile;
            try
            {
                jsonFile = JsonConvert.DeserializeObject<WhosWatchingTVPostProcessingFile>(File.ReadAllText(filePath));
            }
            catch (Exception e)
            {
                errorMessage =
                    "File is in an invalid format.  It cannot be read in its current state; must be a valid JSON file." +
                    "\r\n" + e.ToString();
                return affidavitSaveRequest;
            }

            for (var recordNumber = 0; recordNumber < jsonFile.Details.Count; recordNumber++)
            {
                var jsonDetail = jsonFile.Details[recordNumber];

                var affidavitSaveRequestDetail = new AffidavitSaveRequestDetail()
                {
                    Genre = jsonDetail.Genre,
                    AirTime = jsonDetail.Date.Add(DateTime.Parse(jsonDetail.Time).TimeOfDay),
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
                    InventorySource =
                        (int) (InventorySourceEnum) Enum.Parse(typeof(InventorySourceEnum), jsonDetail.InventorySource),
                    SpotCost = jsonDetail.SpotCost,
                    LeadInEndTime = jsonDetail.Date.Add(DateTime.Parse(jsonDetail.LeadInEndTime).TimeOfDay),
                    LeadOutStartTime = jsonDetail.Date.Add(DateTime.Parse(jsonDetail.LeadOutStartTime).TimeOfDay),
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

        private void _DownloadFileFromWWTVFtp(string fileName, string filePath)
        {
            using (var ftpClient = new WebClient())
            {
                ftpClient.Credentials = new NetworkCredential(BroadcastServiceSystemParameter.WWTV_FtpUsername,
                    BroadcastServiceSystemParameter.WWTV_FtpPassword);
                ftpClient.DownloadFile(
                    $"{FTP_SCHEME}{BroadcastServiceSystemParameter.WWTV_FtpHost}/{BroadcastServiceSystemParameter.WWTV_FtpInboundFolder}/{fileName}",
                    filePath);
            }
        }

        private List<string> _GetWWTVFTPFileNames()
        {
            string uri =
                $"{FTP_SCHEME}{BroadcastServiceSystemParameter.WWTV_FtpHost}/{BroadcastServiceSystemParameter.WWTV_FtpInboundFolder}";
            FtpWebRequest ftpRequest = (FtpWebRequest) WebRequest.Create(uri);
            ftpRequest.Credentials = new NetworkCredential(BroadcastServiceSystemParameter.WWTV_FtpUsername,
                BroadcastServiceSystemParameter.WWTV_FtpPassword);
            ftpRequest.Method = WebRequestMethods.Ftp.ListDirectory;
            FtpWebResponse response = (FtpWebResponse) ftpRequest.GetResponse();
            StreamReader streamReader = new StreamReader(response.GetResponseStream());

            List<string> files = new List<string>();

            string line = streamReader.ReadLine();
            while (!string.IsNullOrWhiteSpace(line))
            {
                if (line.EndsWith(VALID_INCOMING_FILE_EXTENSION))
                {
                    files.Add(line);
                }

                line = streamReader.ReadLine();
            }

            streamReader.Close();
            return files;
        }
    }
}