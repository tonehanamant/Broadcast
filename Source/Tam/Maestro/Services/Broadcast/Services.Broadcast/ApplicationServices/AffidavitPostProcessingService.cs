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
        void DownloadAndProcessWWTVFiles();

        /// <summary>
        /// Process and WWTV post processing file
        /// </summary>
        /// <param name="filePath">Path of the file to process</param>
        /// <returns>BaseResponse object</returns>
        AffidavitSaveRequest ParseWWTVFile(string filePath);
    }


    public class AffidavitPostProcessingService : IAffidavitPostProcessingService
    {
        private static readonly HttpClient client = new HttpClient();
        private readonly IBroadcastAudiencesCache _AudienceCache;
        private readonly IAffidavitValidationEngine _AffidavitValidationEngine;
        private readonly IAffidavitEmailSenderService _AffidavitEmailSenderService;

        private const string VALID_INCOMING_FILE_EXTENSION = ".txt";
        private const string HTTP_ACCEPT_HEADER = "application/json";
        private const string FTP_SCHEME = "ftp://";

        private List<AffidavitValidationResult> _AffidavitValidationResult = new List<AffidavitValidationResult>();

        public AffidavitPostProcessingService(IBroadcastAudiencesCache audienceCache, IDataRepositoryFactory broadcastDataRepositoryFactory, IAffidavitValidationEngine affidavitValidationEngine, IAffidavitEmailSenderService affidavitEmailSenderService)
        {
            _AudienceCache = audienceCache;
            _AffidavitValidationEngine = affidavitValidationEngine;
            _AffidavitEmailSenderService = affidavitEmailSenderService;
        }

        /// <summary>
        /// Downloads the WWTV processed files and calls the affidavit processing service
        /// </summary>
        public void DownloadAndProcessWWTVFiles()
        {
            List<string> filesToProcess = _GetWWTVFTPFileNames();
            foreach (var file in filesToProcess)
            {
                string filePath = $"{Path.GetTempPath()}{file}";
                _DownloadFileFromWWTVFtp(file, filePath);

                AffidavitSaveRequest affidavitFile = ParseWWTVFile(filePath);

                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(HTTP_ACCEPT_HEADER));
                var postResponse = client.PostAsJsonAsync(BroadcastServiceSystemParameter.AffidavitUploadUrl, affidavitFile).GetAwaiter().GetResult();
                var response = JsonConvert.DeserializeObject<BaseResponse<int>>(postResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                if (_AffidavitValidationResult.Count > 0)
                {
                    var invalidFilePath = _MoveFileToInvalidFilesFolder(filePath);

                    var emailBody = _CreateInvalidFileEmailBody(_AffidavitValidationResult, filePath);

                    _AffidavitEmailSenderService.Send(emailBody);
                }

                if (response.Success)
                {
                    _DeleteWWTVFTPFile(Path.GetFileName(filePath));
                }
            }
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

            AffidavitSaveRequest affidavitFile = _MapWWTVFileToAffidavitFile(filePath);
            if(affidavitFile.Details.Count == 0)
            {
                throw new Exception("Invalid file content.");
            }

            affidavitFile.FileHash = HashGenerator.ComputeHash(File.ReadAllBytes(filePath));
            affidavitFile.FileName = Path.GetFileName(filePath);

            return affidavitFile;
        }

        private string _CreateInvalidFileEmailBody(List<AffidavitValidationResult> affidavitValidationResults, string filePath)
        {
            var emailBody = new StringBuilder();

            emailBody.AppendFormat("File {0} failed validation for WWTV upload", Path.GetFileName(filePath));

            foreach (var affidavitValidationResult in affidavitValidationResults)
            {
                emailBody.AppendFormat("Failed validation at line {0} on field {1}", affidavitValidationResult.InvalidLine, affidavitValidationResult.InvalidField);
                emailBody.Append(affidavitValidationResult.ErrorMessage);
            }

            emailBody.AppendFormat("File located in {0}", filePath);

            return emailBody.ToString();
        }

        private string _MoveFileToInvalidFilesFolder(string fileName)
        {
            var combinedFilePath = Path.Combine(BroadcastServiceSystemParameter.WWTV_FailedFolder, Path.GetFileName(fileName));

            File.Move(fileName, combinedFilePath);

            return combinedFilePath;
        }

        private void _DeleteWWTVFTPFile(string fileName)
        {
            string uri = $"{FTP_SCHEME}{BroadcastServiceSystemParameter.WWTV_FtpHost}/{BroadcastServiceSystemParameter.WWTV_FtpInboundFolder}/{fileName}";
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(uri);
            request.Credentials = new NetworkCredential(BroadcastServiceSystemParameter.WWTV_FtpUsername, BroadcastServiceSystemParameter.WWTV_FtpPassword);
            request.Method = WebRequestMethods.Ftp.DeleteFile;

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            response.Close();
        }

        private AffidavitSaveRequest _MapWWTVFileToAffidavitFile(string filePath)
        {
            var jsonFile = JsonConvert.DeserializeObject<WhosWatchingTVPostProcessingFile>(File.ReadAllText(filePath));

            AffidavitSaveRequest file = new AffidavitSaveRequest
            {
                Source = (int)AffidaviteFileSource.Strata,
                Details = new List<AffidavitSaveRequestDetail>()
            };

            for(var lineNumber = 0; lineNumber < jsonFile.Details.Count; lineNumber++)
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
                    InventorySource = (int)(InventorySourceEnum)Enum.Parse(typeof(InventorySourceEnum), jsonDetail.InventorySource),
                    SpotCost = jsonDetail.SpotCost,
                    Demographics = jsonDetail.Demographics.Select(y => new Demographics()
                    {
                        AudienceId = _AudienceCache.GetDisplayAudienceByCode(y.Demographic).Id,
                        OvernightImpressions = y.OvernightImpressions,
                        OvernightRating = y.OvernightRating
                    }).ToList()
                };

                var validationResult = _AffidavitValidationEngine.ValidateAffidavitRecord(affidavitSaveRequestDetail);

                if (!validationResult.Valid)
                {
                    validationResult.InvalidLine = lineNumber;

                    _AffidavitValidationResult.Add(validationResult);

                    continue;
                }

                file.Details.Add(affidavitSaveRequestDetail);
            }

            return file;
        }

        private void _DownloadFileFromWWTVFtp(string fileName, string filePath)
        {
            using (var ftpClient = new WebClient())
            {
                ftpClient.Credentials = new NetworkCredential(BroadcastServiceSystemParameter.WWTV_FtpUsername, BroadcastServiceSystemParameter.WWTV_FtpPassword);
                ftpClient.DownloadFile(
                    $"{FTP_SCHEME}{BroadcastServiceSystemParameter.WWTV_FtpHost}/{BroadcastServiceSystemParameter.WWTV_FtpInboundFolder}/{fileName}",
                    filePath);
            }
        }

        private List<string> _GetWWTVFTPFileNames()
        {
            string uri = $"{FTP_SCHEME}{BroadcastServiceSystemParameter.WWTV_FtpHost}/{BroadcastServiceSystemParameter.WWTV_FtpInboundFolder}";
            FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create(uri);
            ftpRequest.Credentials = new NetworkCredential(BroadcastServiceSystemParameter.WWTV_FtpUsername, BroadcastServiceSystemParameter.WWTV_FtpPassword);
            ftpRequest.Method = WebRequestMethods.Ftp.ListDirectory;
            FtpWebResponse response = (FtpWebResponse)ftpRequest.GetResponse();
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
