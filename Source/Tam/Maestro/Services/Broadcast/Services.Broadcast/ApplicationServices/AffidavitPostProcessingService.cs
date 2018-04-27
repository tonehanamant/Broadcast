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
using EntityFrameworkMapping.Broadcast;
using Tam.Maestro.Common;
using Tam.Maestro.Services.Cable.Entities;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace Services.Broadcast.ApplicationServices
{
    public interface IAffidavitPostProcessingService : IApplicationService
    {
        /// <summary>
        /// Downloads the WWTV processed files and calls the affidavit processing service
        /// </summary>
        void DownloadAndProcessWWTVFiles(string userName, out List<string> filesFailedDownload);

        /// <summary>
        /// Process and WWTV post processing file
        /// </summary>
        /// <param name="filePath">Path of the file to process</param>
        /// <returns>BaseResponse object</returns>
        AffidavitSaveRequest ParseWWTVFile(string filePath);
    }


    public class AffidavitPostProcessingService : IAffidavitPostProcessingService
    {
        private readonly IBroadcastAudiencesCache _AudienceCache;
        private readonly IAffidavitValidationEngine _AffidavitValidationEngine;
        private readonly IAffidavitEmailSenderService _AffidavitEmailSenderService;
        private readonly IAffidavitService _AffidavidService;

        private const string VALID_INCOMING_FILE_EXTENSION = ".txt";
        private const string HTTP_ACCEPT_HEADER = "application/json";
        private const string FTP_SCHEME = "ftp://";

        private string _errorMessage;

        public AffidavitPostProcessingService(
            IBroadcastAudiencesCache audienceCache,
            IDataRepositoryFactory broadcastDataRepositoryFactory,
            IAffidavitValidationEngine affidavitValidationEngine,
            IAffidavitEmailSenderService affidavitEmailSenderService,
            IAffidavitService affidavidService)
        {
            _AudienceCache = audienceCache;
            _AffidavitValidationEngine = affidavitValidationEngine;
            _AffidavitEmailSenderService = affidavitEmailSenderService;
            _AffidavidService = affidavidService;
        }



        /// <summary>
        /// Downloads the WWTV processed files and calls the affidavit processing service
        /// return true if download success, false if download fails (use for loggin)
        /// This involves FTP 
        /// 
        /// </summary>
        public void DownloadAndProcessWWTVFiles(string userName,out List<string> filesFailedDownload)
        {
            filesFailedDownload = new List<string>();
            List<string> filesToProcess = _GetWWTVFTPFileNames();
            foreach (var file in filesToProcess)
            {
                string filePath = $"{Path.GetTempPath()}{file}";
                try
                {
                    _DownloadFileFromWWTVFtp(file, filePath);
                }
                catch 
                {               
                    // cannot download file
                    filesFailedDownload.Add(file);
                    continue;   // skip to next file 
                }
                AffidavitSaveResult response = null;

                AffidavitSaveRequest affidavitFile = ParseWWTVFile(filePath);
                                
                if (affidavitFile == null)
                {
                    _ProcessError(filePath);
                    return;
                }
                try
                {
                    response = _AffidavidService.SaveAffidavit(affidavitFile, userName, DateTime.Now);
                }
                catch (Exception e)
                {
                    _errorMessage = "Error saving affidavit:\n\n" + e.ToString();
                    _ProcessError(filePath);
                    return;
                }

                _DeleteWWTVFTPFile(Path.GetFileName(filePath));
            }
        }

        private void _ProcessError(string filePath)
        {
            var invalidFilePath = _MoveFileToInvalidFilesFolder(filePath);

            var emailBody = _CreateInvalidFileEmailBody(_errorMessage, invalidFilePath);

            _AffidavitEmailSenderService.Send(emailBody);

            _DeleteWWTVFTPFile(Path.GetFileName(filePath));
        }

        /// <summary>
        /// Process and WWTV post processing file
        /// </summary>
        /// <param name="filePath">Path of the file to process</param>
        /// <returns>BaseResponse object containing the new id of the affidavit_file</returns>
        public AffidavitSaveRequest ParseWWTVFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new Exception("File does not exist.");
            }

            if (!Path.GetExtension(filePath).Equals(".txt"))
            {
                throw new Exception("Invalid file extension.");
            }

            AffidavitSaveRequest affidavitFile;
            try
            {
                affidavitFile = _MapWWTVFileToAffidavitFile(filePath);
                if (affidavitFile.Details.Count == 0)
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                _errorMessage = "Could not process file.\n  " + e.ToString();
                return null;
            }

            affidavitFile.FileHash = HashGenerator.ComputeHash(File.ReadAllBytes(filePath));
            affidavitFile.FileName = Path.GetFileName(filePath);

            return affidavitFile;
        }

        private string _CreateInvalidFileEmailBody(string errorMessage,
            string filePath)
        {
            var emailBody = new StringBuilder();

            emailBody.AppendFormat("File {0} failed validation for WWTV upload\n\n{1}", Path.GetFileName(filePath), errorMessage);

            emailBody.AppendFormat("\n\nFile located in {0}\n", filePath);

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

        private AffidavitSaveRequest _MapWWTVFileToAffidavitFile(string filePath)
        {
            AffidavitSaveRequest file = new AffidavitSaveRequest
            {
                Source = (int) AffidaviteFileSource.Strata,
                Details = new List<AffidavitSaveRequestDetail>()
            };

            WhosWatchingTVPostProcessingFile jsonFile;
            try
            {
                jsonFile = JsonConvert.DeserializeObject<WhosWatchingTVPostProcessingFile>(File.ReadAllText(filePath));
            }
            catch (Exception e)
            {
                _errorMessage =
                    "File is in an invalid format.  It cannot be read in its current state; must be a valid JSON file." +
                    "\r\n" + e.ToString();
                return file;
            }

            for (var lineNumber = 0; lineNumber < jsonFile.Details.Count; lineNumber++)
            {
                var jsonDetail = jsonFile.Details[lineNumber];

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

                file.Details.Add(affidavitSaveRequestDetail);
            }

            return file;
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