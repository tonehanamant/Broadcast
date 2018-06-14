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
        List<OutboundAffidavitFileValidationResultDto> ProcessFiles(string userName);
        List<OutboundAffidavitFileValidationResultDto> ValidateFiles(List<string> filepathList, string userName);
        void ProcessErrorFiles();
    }

    public class AffidavitPreprocessingService : IAffidavitPreprocessingService
    {
        internal List<string> AffidavitFileHeaders = new List<string>() { "ESTIMATE_ID", "STATION_NAME", "DATE_RANGE", "SPOT_TIME", "SPOT_DESCRIPTOR", "COST" };

        public const string _ValidStrataExtension = ".xlsx";
        public const string _ValidStrataTabName = "PostAnalRep_ExportDetail";

        private readonly IAffidavitRepository _AffidavitRepository;
        private readonly IDataRepositoryFactory _BroadcastDataRepositoryFactory;
        private readonly IAffidavitEmailProcessorService _affidavitEmailProcessorService;
        
        public AffidavitPreprocessingService(IDataRepositoryFactory broadcastDataRepositoryFactory, IAffidavitEmailProcessorService affidavitEmailProcessorService)
        {
            _BroadcastDataRepositoryFactory = broadcastDataRepositoryFactory;
            _AffidavitRepository = _BroadcastDataRepositoryFactory.GetDataRepository<IAffidavitRepository>();
            _affidavitEmailProcessorService = affidavitEmailProcessorService;
        }

        /// <summary>
        /// Checks if all the files are valid according to Strata file validation rules
        /// </summary>
        /// <param name="filepathList">List of filepaths</param>
        /// <param name="userName">User processing the files</param>
        /// <returns>List of ValidationFileResponseDto objects</returns>
        public List<OutboundAffidavitFileValidationResultDto> ProcessFiles(string userName)
        {
            List<string> filepathList;
            List<OutboundAffidavitFileValidationResultDto> validationList = new List<OutboundAffidavitFileValidationResultDto>();
            WWTVSharedNetworkHelper.Impersonate(delegate 
            {
                filepathList = GetDropFolderFileList();
                validationList = ValidateFiles(filepathList, userName);
                _AffidavitRepository.SaveValidationObject(validationList);
                var validFileList = validationList.Where(v => v.Status == AffidaviteFileProcessingStatus.Valid)
                    .ToList();
                if (validFileList.Any())
                {
                    this._CreateAndUploadZipArchiveToWWTV(validFileList);
                }

                _affidavitEmailProcessorService.ProcessAndSendInvalidDataFiles(validationList);
                DeleteSuccessfulFiles(validationList);
            });

            return validationList;
        }

        private static List<string> GetDropFolderFileList()
        {
            List<string> filepathList;
            try
            {
                var dropFolder = WWTVSharedNetworkHelper.GetLocalDropFolder();
                filepathList = Directory.GetFiles(dropFolder).ToList();
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Could not find WWTV_SharedFolder.  Please check it is created.", e);
            }

            return filepathList;
        }

        private static void DeleteSuccessfulFiles(List<OutboundAffidavitFileValidationResultDto> validationList)
        {
            validationList.ForEach(r =>
            {
                if (r.Status == AffidaviteFileProcessingStatus.Valid)
                {
                    File.Delete(r.FilePath);
                }
            });
        }


        /// <summary>
        /// Creates and uploads a zip archive to WWTV FTP server
        /// </summary>
        /// <param name="files">List of OutboundAffidavitFileValidationResultDto objects representing the valid files to be sent</param>
        private void _CreateAndUploadZipArchiveToWWTV(List<OutboundAffidavitFileValidationResultDto> files)
        {
            string zipFileName = WWTVSharedNetworkHelper.GetLocalErrorFolder();
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


        public void ProcessErrorFiles()
        {
            var remoteFTPPath = WWTVFtpHelper.GetFTPErrorPath();

            WWTVSharedNetworkHelper.Impersonate(delegate
            {
                var files = WWTVFtpHelper.GetFileList(remoteFTPPath);
                var localPaths = new List<string>();
                var completedFiles = _DownloadFTPFiles(files, remoteFTPPath, ref localPaths);
                EmailFTPErrorFiles(localPaths);
            });
        }

        private void EmailFTPErrorFiles(List<string> filePaths)
        {
            if (!BroadcastServiceSystemParameter.EmailNotificationsEnabled)
                return;

            filePaths.ForEach(filePath =>
            {
                var body = string.Format("{0}",Path.GetFileName(filePath));
                var subject = "Error files from WWTV";
                var from = BroadcastServiceSystemParameter.EmailUsername;
                var Tos = new string[] { BroadcastServiceSystemParameter.WWTV_NotificationEmail };
                Emailer.QuickSend(true, body, subject, MailPriority.Normal,from , Tos, new List<string>() {filePath });
            });
        }

        private List<string> _DownloadFTPFiles(List<string> files, string remoteFTPPath,ref List<string> localFilePaths)
        {
            var local = new List<string>();
            List<string> completedFiles = new List<string>();
            using (var ftpClient = new WebClient())
            {
                ftpClient.Credentials = WWTVFtpHelper.GetFtpClientCredentials();

                var localFolder = WWTVSharedNetworkHelper.GetLocalErrorFolder();
                files.ForEach(filePath =>
                {
                    var path = remoteFTPPath + "/" + filePath.Remove(0, filePath.IndexOf(@"/") + 1);
                    var localPath = localFolder + @"\" + filePath.Replace(@"/", @"\");
                    if (File.Exists(localPath))
                        File.Delete(localPath);

                    ftpClient.DownloadFile(path, localPath);
                    local.Add(localPath);
                    WWTVFtpHelper.DeleteFile(path);
                    completedFiles.Add(path);
                });
            }

            localFilePaths = local;
            return completedFiles;
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

        private void _UploadZipToWWTV(string zipFileName)
        {
            var sharedFolder = WWTVFtpHelper.GetFTPOutboundPath();
            var uploadUrl = $"{sharedFolder}/{Path.GetFileName(zipFileName)}";
            WWTVFtpHelper.UploadFile(zipFileName, uploadUrl,File.Delete); 
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
                    SourceId = (int)AffidaviteFileSourceEnum.Strata,
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
                    currentFile.Status = AffidaviteFileProcessingStatus.Valid;
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
                currentFile.Status = AffidaviteFileProcessingStatus.Invalid;
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
                currentFile.Status = AffidaviteFileProcessingStatus.Invalid;
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
                currentFile.Status = AffidaviteFileProcessingStatus.Invalid;
            }
            return tab;
        }

        private void _ValidateStrataFileExtension(OutboundAffidavitFileValidationResultDto currentFile)
        {
            if (!Path.GetExtension(currentFile.FilePath).Equals(_ValidStrataExtension, StringComparison.InvariantCultureIgnoreCase))
            {
                currentFile.ErrorMessages.Add($"Invalid extension for file {currentFile.FilePath}");
                currentFile.Status = AffidaviteFileProcessingStatus.Invalid;
            }
        }

        private bool _IsEmptyRow(int row, ExcelWorksheet excelWorksheet)
        {
            for (var c = 1; c < excelWorksheet.Dimension.End.Column; c++)
                if (!string.IsNullOrWhiteSpace(excelWorksheet.Cells[row, c].Text))
                    return false;

            return true;
        }
    }
}
