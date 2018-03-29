using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Newtonsoft.Json;
using OfficeOpenXml;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using Tam.Maestro.Common;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace Services.Broadcast.ApplicationServices
{
    public interface IAffidavitPreprocessingService : IApplicationService
    {
        /// <summary>
        /// Checks if all the files are valid according to Strata file validation rules
        /// </summary>
        /// <param name="filepathList">List of filepaths</param>
        /// <param name="userName">User processing the files</param>
        /// <returns>List of OutboundAffidavitFileValidationResultDto objects</returns>
        List<OutboundAffidavitFileValidationResultDto> ProcessFiles(List<string> filepathList, string userName);
        List<OutboundAffidavitFileValidationResultDto> ValidateFiles(List<string> filepathList, string userName);

        /// <summary>
        /// Creates and uploads a zip archive to WWTV FTP server
        /// </summary>
        /// <param name="files">List of OutboundAffidavitFileValidationResultDto objects representing the valid files to be sent</param>
        void CreateAndUploadZipArchiveToWWTV(List<OutboundAffidavitFileValidationResultDto> files);

        void ProcessErrorFiles();


        /// <summary>
        /// Move invalid files to invalid files folder. Notify users about failed files
        /// </summary>
        /// <param name="files">List of OutboundAffidavitFileValidationResultDto objects representing the valid files to be sent</param>
        void ProcessInvalidFiles(List<OutboundAffidavitFileValidationResultDto> files);
    }

    public enum AffidaviteFileProcessingStatus
    {
        Valid = 1,
        Invalid = 2
    };

    public class AffidavitPreprocessingService : IAffidavitPreprocessingService
    {
        internal List<string> AffidavitFileHeaders = new List<string>() { "ESTIMATE_ID", "STATION_NAME", "DATE_RANGE", "SPOT_TIME", "SPOT_DESCRIPTOR", "COST" };

        public readonly string _ValidStrataExtension = ".xlsx";
        public readonly string _ValidStrataTabName = "PostAnalRep_ExportDetail";

        private readonly IAffidavitPreprocessingRepository _AffidavitPreprocessingRepository;
        private readonly IDataRepositoryFactory _BroadcastDataRepositoryFactory;
        private readonly IAffidavitEmailSenderService _AffidavitEmailSenderService;
        
        public AffidavitPreprocessingService(IDataRepositoryFactory broadcastDataRepositoryFactory, IAffidavitEmailSenderService affidavitEmailSenderService)
        {
            _BroadcastDataRepositoryFactory = broadcastDataRepositoryFactory;
            _AffidavitPreprocessingRepository = _BroadcastDataRepositoryFactory.GetDataRepository<IAffidavitPreprocessingRepository>();
            _AffidavitEmailSenderService = affidavitEmailSenderService;
        }

        /// <summary>
        /// Checks if all the files are valid according to Strata file validation rules
        /// </summary>
        /// <param name="filepathList">List of filepaths</param>
        /// <param name="userName">User processing the files</param>
        /// <returns>List of ValidationFileResponseDto objects</returns>
        public List<OutboundAffidavitFileValidationResultDto> ProcessFiles(List<string> filepathList, string userName)
        {
            List<OutboundAffidavitFileValidationResultDto> validationList = ValidateFiles(filepathList, userName);
            _AffidavitPreprocessingRepository.SaveValidationObject(validationList);
            var validFileList = validationList.Where(v => v.Status == (int)AffidaviteFileProcessingStatus.Valid)
                                                .ToList();

            if (validFileList.Any())
                this.CreateAndUploadZipArchiveToWWTV(validFileList);

            return validationList;
        }

        /// <summary>
        /// Move invalid files to invalid files folder. Notify users about failed files
        /// </summary>
        /// <param name="files">List of OutboundAffidavitFileValidationResultDto objects representing the valid files to be sent</param>
        public void ProcessInvalidFiles(List<OutboundAffidavitFileValidationResultDto> validationList)
        {
            var invalidFiles = validationList.Where(v => v.Status == (int)AffidaviteFileProcessingStatus.Invalid);

            foreach (var invalidFile in invalidFiles)
            {
                var invalidFilePath = _MoveInvalidFileToArchiveFolder(invalidFile);

                var emailBody = _CreateInvalidFileEmailBody(invalidFile, invalidFilePath);

                _AffidavitEmailSenderService.Send(emailBody);
            }
        }

        /// <summary>
        /// Creates and uploads a zip archive to WWTV FTP server
        /// </summary>
        /// <param name="files">List of OutboundAffidavitFileValidationResultDto objects representing the valid files to be sent</param>
        public void CreateAndUploadZipArchiveToWWTV(List<OutboundAffidavitFileValidationResultDto> files)
        {
            string zipFileName = Path.GetTempPath();
            if (!zipFileName.EndsWith("\\"))
                zipFileName += "\\";
            zipFileName += "Post_" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".zip";
            _CreateZipArchive(files, zipFileName);
            if (File.Exists(zipFileName))
            {
                _UploadZipToWWTV(zipFileName);
                File.Delete(zipFileName);
            }
        }

        private string _CreateInvalidFileEmailBody(OutboundAffidavitFileValidationResultDto invalidFile, string invalidFilePath)
        {
            var mailBody = new StringBuilder();

            mailBody.AppendFormat("File {0} failed validation for WWTV upload", invalidFile.FileName);

            foreach (var errorMessage in invalidFile.ErrorMessages)
            {
                mailBody.Append(errorMessage);
            }

            mailBody.AppendFormat("File located in {0}", invalidFilePath);

            return mailBody.ToString();
        }

        private string _MoveInvalidFileToArchiveFolder(OutboundAffidavitFileValidationResultDto invalidFile)
        {
            var combinedFilePath = Path.Combine(BroadcastServiceSystemParameter.WWTV_FailedFolder, Path.GetFileName(invalidFile.FilePath));

            File.Move(invalidFile.FilePath, combinedFilePath);

            return combinedFilePath;
        }

        public void ProcessErrorFiles()
        {
            var remoteFTPPath =
                $"ftp://{BroadcastServiceSystemParameter.WWTV_FtpHost}/{BroadcastServiceSystemParameter.WWTV_FtpErrorFolder}";

            var files = GetFTPFileList(remoteFTPPath);
            var completedFiles = DownloadFTPFiles(files,remoteFTPPath);
            EmailFTPErrorFiles(completedFiles);
        }

        private void EmailFTPErrorFiles(List<string> filePaths)
        {
            if (!BroadcastServiceSystemParameter.EmailNotificationsEnabled)
                return;

            filePaths.ForEach(filePath =>
            {
                var body = "Error file!";
                var subject = "Error files from WWTV";
                var from = BroadcastServiceSystemParameter.EmailFrom;
                var Tos = new string[] { BroadcastServiceSystemParameter.WWTV_NotificationEmail };
                Emailer.QuickSend(true, body, subject, MailPriority.Normal,from , Tos, new List<string>() { filePath} );
            });
            
        }

        private List<string> DownloadFTPFiles(List<string> files, string remoteFTPPath)
        {
            List<string> completedFiles = new List<string>();
            using (var ftpClient = new WebClient())
            {
                ftpClient.Credentials = GetFtpClientCredentials();

                files.ForEach(filePath =>
                {
                    var path = remoteFTPPath + "/" + filePath.Remove(0, filePath.IndexOf(@"/") + 1);
                    var transferPath = BroadcastServiceSystemParameter.WWTV_LocalFtpErrorFolder + @"\" +
                                        filePath.Replace(@"/", @"\");
                    ftpClient.DownloadFile(path, transferPath);

                    DeleteFTPFile(path);
                    completedFiles.Add(path);
                });
            }

            return completedFiles;
        }

        private static List<string> GetFTPFileList(string remoteFTPPath)
        {
            var request = (FtpWebRequest) WebRequest.Create(remoteFTPPath);

            request.Method = WebRequestMethods.Ftp.ListDirectory;
            request.Credentials = GetFtpClientCredentials();

            FtpWebResponse response = (FtpWebResponse) request.GetResponse();
            Stream responseStream = response.GetResponseStream();
            List<string> files = new List<string>();

            using (StreamReader reader = new StreamReader(responseStream))
            {
                var line = reader.ReadLine();

                while (!string.IsNullOrEmpty(line))
                {
                    files.Add(line);
                    line = reader.ReadLine();
                }
            }
            return files;
        }


        private static void _CreateZipArchive(List<OutboundAffidavitFileValidationResultDto> files, string zipFileName)
        {
            using (ZipArchive zip = ZipFile.Open(zipFileName, ZipArchiveMode.Create))
            {
                foreach (var file in files)
                {
                    // Add the entry for each file
                    zip.CreateEntryFromFile(file.FilePath, Path.GetFileName(file.FilePath), System.IO.Compression.CompressionLevel.NoCompression);
                }
            }
        }

        private void _UploadZipToWWTV(string zipFilePath)
        {
            using (var ftpClient = new WebClient())
            {
                ftpClient.Credentials = GetFtpClientCredentials();
                var uploadUrl = $"ftp://{BroadcastServiceSystemParameter.WWTV_FtpHost}/{BroadcastServiceSystemParameter.WWTV_FtpOutboundFolder}/{Path.GetFileName(zipFilePath)}";
                ftpClient.UploadFile(uploadUrl,zipFilePath);
            }
        }

        public List<OutboundAffidavitFileValidationResultDto> ValidateFiles(List<string> filepathList, string userName)
        {
            List<OutboundAffidavitFileValidationResultDto> result = new List<OutboundAffidavitFileValidationResultDto>();

            foreach (var filepath in filepathList)
            {
                OutboundAffidavitFileValidationResultDto currentFile = new OutboundAffidavitFileValidationResultDto()
                {
                    FilePath = filepath,
                    FileName = Path.GetFileName(filepath),
                    SourceId = (int)AffidaviteFileSource.Strata,
                    CreatedBy = userName,
                    CreatedDate = DateTime.Now,
                    FileHash = HashGenerator.ComputeHash(File.ReadAllBytes(filepath))
                };
                result.Add(currentFile);

                var fileInfo = new FileInfo(filepath);

                //check if valid extension                
                _ValidateStrataFileExtension(currentFile);
                if (currentFile.ErrorMessages.Any())
                    continue;

                //check if tab exists
                ExcelWorksheet tab = _ValidateWorksheetName(fileInfo, currentFile);
                if (currentFile.ErrorMessages.Any())
                    continue;

                //check column headers
                Dictionary<string, int> headers = _ValidateHeaders(currentFile, tab);
                if (currentFile.ErrorMessages.Any())
                    continue;

                //check required data fields
                _HasMissingData(tab, headers, currentFile);
                if (currentFile.ErrorMessages.Any())
                    continue;

                if (!currentFile.ErrorMessages.Any())
                    currentFile.Status = (int)AffidaviteFileProcessingStatus.Valid;
            }

            return result;
        }

        private void _HasMissingData(ExcelWorksheet tab, Dictionary<string, int> headers, OutboundAffidavitFileValidationResultDto currentFile)
        {
            var hasMissingData = false;
            for (var row = 2; row <= tab.Dimension.End.Row; row++)
            {
                if (_IsEmptyRow(row, tab))
                {
                    continue;
                }
                foreach (string name in AffidavitFileHeaders)
                {
                    if (string.IsNullOrWhiteSpace(tab.Cells[row, headers[name]].Value?.ToString()))
                    {
                        currentFile.ErrorMessages.Add($"Missing {name} on row {row}");
                        hasMissingData = true;
                    }
                }
            }
            if (hasMissingData)
            {
                currentFile.Status = (int)AffidaviteFileProcessingStatus.Invalid;
            }
        }

        private Dictionary<string, int> _ValidateHeaders(OutboundAffidavitFileValidationResultDto currentFile, ExcelWorksheet tab)
        {
            var headers = new Dictionary<string, int>();
            foreach (var header in AffidavitFileHeaders)
            {
                for (var column = 1; column <= tab.Dimension.End.Column; column++)
                {
                    var cellValue = (string)tab.Cells[1, column].Value;

                    if (!cellValue.Trim().Equals(header, StringComparison.InvariantCultureIgnoreCase))
                        continue;

                    headers.Add(header, column);
                    break;
                }

                if (!headers.ContainsKey(header))
                {
                    currentFile.ErrorMessages.Add(string.Format("Could not find header for column {0} in file {1}", header, currentFile.FilePath));
                }
            }
            if (headers.Count != AffidavitFileHeaders.Count)
            {
                currentFile.Status = (int)AffidaviteFileProcessingStatus.Invalid;
            }
            return headers;
        }

        private ExcelWorksheet _ValidateWorksheetName(FileInfo fileInfo, OutboundAffidavitFileValidationResultDto currentFile)
        {
            ExcelWorksheet tab = null;
            var package = new ExcelPackage(fileInfo, true);
            foreach (var worksheet in package.Workbook.Worksheets)
            {
                if (worksheet.Name.Equals(_ValidStrataTabName))
                {
                    tab = worksheet;
                }
            }
            if (tab == null)
            {
                currentFile.ErrorMessages.Add(string.Format("Could not find the tab {0} in file {1}", _ValidStrataTabName, currentFile.FilePath));
                currentFile.Status = (int)AffidaviteFileProcessingStatus.Invalid;
            }
            return tab;
        }

        private void _ValidateStrataFileExtension(OutboundAffidavitFileValidationResultDto currentFile)
        {
            if (!Path.GetExtension(currentFile.FilePath).Equals(_ValidStrataExtension, StringComparison.InvariantCultureIgnoreCase))
            {
                currentFile.ErrorMessages.Add($"Invalid extension for file {currentFile.FilePath}");
                currentFile.Status = (int)AffidaviteFileProcessingStatus.Invalid;
            }
        }

        private bool _IsEmptyRow(int row, ExcelWorksheet excelWorksheet)
        {
            for (var c = 1; c < excelWorksheet.Dimension.End.Column; c++)
                if (!string.IsNullOrWhiteSpace(excelWorksheet.Cells[row, c].Text))
                    return false;

            return true;
        }

        private void DeleteFTPFile(string remoteFTPPath)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(remoteFTPPath);

            request.Method = WebRequestMethods.Ftp.DeleteFile;
            request.Credentials = GetFtpClientCredentials();
            request.Proxy = null;

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            response.Close();
        }
        private static NetworkCredential GetFtpClientCredentials()
        {
            return new NetworkCredential(BroadcastServiceSystemParameter.WWTV_FtpUsername,
                BroadcastServiceSystemParameter.WWTV_FtpPassword);
        }

    }
}
