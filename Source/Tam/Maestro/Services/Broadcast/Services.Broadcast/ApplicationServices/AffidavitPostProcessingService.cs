using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Newtonsoft.Json;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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

        private const string VALID_INCOMING_FILE_EXTENSION = ".txt";
        private const string HTTP_ACCEPT_HEADER = "application/json";
        private const string FTP_SCHEME = "ftp://";

        public AffidavitPostProcessingService(IBroadcastAudiencesCache audienceCache, IDataRepositoryFactory broadcastDataRepositoryFactory)
        {
            _AudienceCache = audienceCache;
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
                Details = jsonFile.Details.Select(x => new AffidavitSaveRequestDetail()
                {
                    Genre = x.Genre,
                    AirTime = x.Date.Add(DateTime.Parse(x.Time).TimeOfDay),
                    Isci = x.ISCI,
                    LeadInGenre = x.LeadInGenre,
                    LeadInProgramName = x.LeadInProgram,
                    LeadOutGenre = x.LeadOutGenre,
                    LeadOutProgramName = x.LeadOutProgram,
                    Market = x.Market,
                    ProgramName = x.Program,
                    SpotLength = x.SpotLength,
                    Station = x.Station,
                    Affiliate = x.Affiliate,
                    EstimateId = x.EstimateId,
                    InventorySource = (int)(InventorySourceEnum)Enum.Parse(typeof(InventorySourceEnum), x.InventorySource),
                    SpotCost = x.SpotCost,
                    Demographics = x.Demographics.Select(y => new Demographics()
                    {
                        AudienceId = _AudienceCache.GetDisplayAudienceByCode(y.Demographic).Id,
                        OvernightImpressions = y.OvernightImpressions,
                        OvernightRating = y.OvernightRating
                    }).ToList()
                }).ToList()
            };
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
