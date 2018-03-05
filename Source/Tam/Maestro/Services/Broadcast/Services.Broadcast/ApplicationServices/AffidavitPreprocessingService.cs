using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using OfficeOpenXml;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
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

        public AffidavitPreprocessingService(IDataRepositoryFactory broadcastDataRepositoryFactory)
        {
            _BroadcastDataRepositoryFactory = broadcastDataRepositoryFactory;
            _AffidavitPreprocessingRepository = _BroadcastDataRepositoryFactory.GetDataRepository<IAffidavitPreprocessingRepository>();
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

            string zipFileName = $@".\Files\Post_{DateTime.Now.ToString("yyyyMMddhhmmss")}.zip";
            _CreateZipArchive(validationList.Where(x => x.Status == (int)AffidaviteFileProcessingStatus.Valid).ToList(), zipFileName);
            if (File.Exists(zipFileName))
            {
                _UploadZipToWWTV(zipFileName);
                File.Delete(zipFileName);
            }
            return validationList;
        }

        private void _UploadZipToWWTV(string zipFilePath)
        {
            using (var ftpClient = new WebClient())
            {
                ftpClient.Credentials = new NetworkCredential(BroadcastServiceSystemParameter.WWTV_FtpUsername, BroadcastServiceSystemParameter.WWTV_FtpPassword);
                ftpClient.UploadFile(
                    $"ftp://{BroadcastServiceSystemParameter.WWTV_FtpHost}/{BroadcastServiceSystemParameter.WWTV_FtpOutboundFolder}/{Path.GetFileName(zipFilePath)}", 
                    zipFilePath);
            }
        }

        private void _CreateZipArchive(List<OutboundAffidavitFileValidationResultDto> filelist, string zipName)
        {
            using (ZipArchive zip = ZipFile.Open(zipName, ZipArchiveMode.Create))
            {
                foreach (var file in filelist)
                {
                    // Add the entry for each file
                    zip.CreateEntryFromFile(file.FilePath, Path.GetFileName(file.FilePath), System.IO.Compression.CompressionLevel.Optimal);
                }
            }
        }

        private List<OutboundAffidavitFileValidationResultDto> ValidateFiles(List<string> filepathList, string userName)
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
    }
}
