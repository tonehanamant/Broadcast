using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Common.Services;
using Tam.Maestro.Common;
using Tam.Maestro.Services.Cable.SystemComponentParameters;
using Services.Broadcast.ApplicationServices.Helpers;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Helpers;

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
        List<WWTVOutboundFileValidationResult> ProcessFiles(string userName);

        /// <summary>
        /// Validates a list of files
        /// </summary>
        /// <param name="filepathList">List of files to validate</param>
        /// <param name="userName">Username requesting the operation</param>
        /// <returns>List of WWTVOutboundFileValidationResult objects</returns>
        List<WWTVOutboundFileValidationResult> ValidateFiles(List<string> filepathList, string userName);        
    }

    public class AffidavitPreprocessingService : IAffidavitPreprocessingService
    {
        private readonly IAffidavitRepository _AffidavitRepository;
        private readonly IDataRepositoryFactory _BroadcastDataRepositoryFactory;
        private readonly IWWTVEmailProcessorService _affidavitEmailProcessorService;
        private readonly IEmailerService _EmailerService;
        private readonly IFileService _FileService;
        private readonly IWWTVFtpHelper _WWTVFtpHelper;
        private readonly IWWTVSharedNetworkHelper _WWTVSharedNetworkHelper;
        private readonly IExcelHelper _ExcelHelper;
        private readonly ICsvHelper _CsvHelper;

        private const string _ValidStrataTabName = "PostAnalRep_ExportDetail";
        private const string _ValidKeepingTracTabName = "KeepingTrac.csv";

        public AffidavitPreprocessingService(IDataRepositoryFactory broadcastDataRepositoryFactory
                                                , IWWTVEmailProcessorService affidavitEmailProcessorService
                                                , IWWTVFtpHelper WWTVFtpHelper
                                                , IWWTVSharedNetworkHelper WWTVSharedNetworkHelper
                                                , IEmailerService emailerService
                                                , IFileService fileService
                                                , IExcelHelper excelHelper
                                                , ICsvHelper csvHelper)
        {
            _BroadcastDataRepositoryFactory = broadcastDataRepositoryFactory;
            _AffidavitRepository = _BroadcastDataRepositoryFactory.GetDataRepository<IAffidavitRepository>();
            _affidavitEmailProcessorService = affidavitEmailProcessorService;
            _WWTVFtpHelper = WWTVFtpHelper;
            _EmailerService = emailerService;
            _WWTVSharedNetworkHelper = WWTVSharedNetworkHelper;
            _FileService = fileService;
            _ExcelHelper = excelHelper;
            _CsvHelper = csvHelper;
        }

        /// <summary>
        /// Checks if all the files are valid according to Strata file validation rules
        /// </summary>
        /// <param name="filepathList">List of filepaths</param>
        /// <param name="userName">User processing the files</param>
        /// <returns>List of ValidationFileResponseDto objects</returns>
        public List<WWTVOutboundFileValidationResult> ProcessFiles(string userName)
        {
            List<string> filepathList;
            List<WWTVOutboundFileValidationResult> validationList = new List<WWTVOutboundFileValidationResult>();
            _WWTVSharedNetworkHelper.Impersonate(delegate
            {
                filepathList = _FileService.GetFiles(WWTVSharedNetworkHelper.GetLocalDropFolder());
                validationList = ValidateFiles(filepathList, userName);
                _AffidavitRepository.SaveValidationObject(validationList);
                var validFileList = validationList.Where(v => v.Status == FileProcessingStatusEnum.Valid).Select(x => x.FilePath).ToList();
                if (validFileList.Any())
                {
                    _CreateAndUploadZipArchiveToWWTV(validFileList);
                    _FileService.Delete(validFileList.ToArray());
                }

                _affidavitEmailProcessorService.ProcessAndSendInvalidDataFiles(validationList.Where(v => v.Status == FileProcessingStatusEnum.Invalid).ToList());

            });

            return validationList;
        }

        /// <summary>
        /// Creates and uploads zip archives to WWTV FTP server
        /// </summary>
        /// <param name="files">List of file paths objects representing the valid files to be sent</param>
        private void _CreateAndUploadZipArchiveToWWTV(List<string> filePaths)
        {
            string zipPath = WWTVSharedNetworkHelper.GetLocalErrorFolder();
            if (!zipPath.EndsWith("\\"))
                zipPath += "\\";

            var strataFiles = filePaths.Where(x => Path.GetExtension(x).Equals(".xlsx")).ToList();
            if (strataFiles.Any())
            {
                string strataZipFile = zipPath + "Post_Affidavit_" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".zip";
                _FileService.CreateZipArchive(strataFiles, strataZipFile);
                _UploadZipToWWTV(strataZipFile);
            }

            var sigmaFiles = filePaths.Where(x => Path.GetExtension(x).Equals(".csv")).ToList();
            if (sigmaFiles.Any())
            {
                string sigmaZipFile = zipPath + "Post_Sigma_" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".zip";
                _FileService.CreateZipArchive(sigmaFiles, sigmaZipFile);
                _UploadZipToWWTV(sigmaZipFile);
            }

            //var keepingTracFiles = filePaths.Where(x => Path.GetExtension(x).Equals(".csv")).ToList();
            //if (keepingTracFiles.Any())
            //{
            //    string keepingTracZipFile = zipPath + "Post_KeepingTrac_" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".zip";
            //    _FileService.CreateZipArchive(keepingTracFiles, keepingTracZipFile);
            //    _UploadZipToWWTV(keepingTracZipFile);
            //}
        }

        private void _UploadZipToWWTV(string zipFileName)
        {
            if (_FileService.Exists(zipFileName))
            {
                var sharedFolder = _WWTVFtpHelper.GetRemoteFullPath(BroadcastServiceSystemParameter.WWTV_FtpOutboundFolder);
                var uploadUrl = $"{sharedFolder}/{Path.GetFileName(zipFileName)}";
                _WWTVFtpHelper.UploadFile(zipFileName, uploadUrl, File.Delete);
                _FileService.Delete(zipFileName);
            }
        }

        /// <summary>
        /// Validates a list of files
        /// </summary>
        /// <param name="filepathList">List of files to validate</param>
        /// <param name="userName">Username requesting the operation</param>
        /// <returns>List of WWTVOutboundFileValidationResult objects</returns>
        public List<WWTVOutboundFileValidationResult> ValidateFiles(List<string> filepathList, string userName)
        {
            List<WWTVOutboundFileValidationResult> result = new List<WWTVOutboundFileValidationResult>();

            foreach (var filepath in filepathList)
            {
                WWTVOutboundFileValidationResult currentFile = new WWTVOutboundFileValidationResult()
                {
                    FilePath = filepath,
                    FileName = Path.GetFileName(filepath),
                    CreatedBy = userName,
                    CreatedDate = DateTime.Now,
                    FileHash = HashGenerator.ComputeHash(File.ReadAllBytes(filepath)),
                    Status = FileProcessingStatusEnum.Invalid
                };
                result.Add(currentFile);

                FileInfo fileInfo = new FileInfo(filepath);
                if (fileInfo.Extension.Equals(".xlsx", StringComparison.InvariantCultureIgnoreCase))
                {
                    _ProcessExcelFile(currentFile, fileInfo);
                }
                else
                {
                    if (fileInfo.Extension.Equals(".csv", StringComparison.InvariantCultureIgnoreCase))
                    {
                        _ProcessCsvFile(currentFile, filepath);
                    }
                    else
                    {
                        currentFile.ErrorMessages.Add($"Unknown extension type for file {filepath}");
                    }
                }
                
                if (!currentFile.ErrorMessages.Any())
                {
                    currentFile.Status = FileProcessingStatusEnum.Valid;
                }
            }

            return result;
        }

        private void _ProcessCsvFile(WWTVOutboundFileValidationResult currentFile, string filepath)
        {
            List<string> requiredSigmaColumns = new List<string>() { "IDENTIFIER 1", "STATION", "DATE AIRED", "AIR START TIME", "ISCI/AD-ID" };
            currentFile.Source = FileSourceEnum.Strata;
            var parser = _CsvHelper.SetupCSVParser(filepath, currentFile);
            if (currentFile.ErrorMessages.Any()) return;

            var fileColumns = _CsvHelper.GetFileHeader(parser);
            currentFile.ErrorMessages.AddRange(_CsvHelper.ValidateRequiredColumns(fileColumns, requiredSigmaColumns));
            if (currentFile.ErrorMessages.Any()) return;

            var headers = _CsvHelper.GetHeaderDictionary(fileColumns, requiredSigmaColumns);

            int rowNumber = 1;
            while (!parser.EndOfData)
            {
                var fields = parser.ReadFields();
                var rowValidationErrors = _CsvHelper.GetRowValidationResults(fields, headers, requiredSigmaColumns, rowNumber);
                currentFile.ErrorMessages.AddRange(rowValidationErrors);
                rowNumber++;
            }
        }

        private void _ProcessExcelFile(WWTVOutboundFileValidationResult currentFile, FileInfo fileInfo)
        {
            List<string> RequiredStrataColumns = new List<string>() { "ESTIMATE_ID", "STATION_NAME", "DATE_RANGE", "SPOT_TIME", "SPOT_DESCRIPTOR", "COST" };
            //List<string> RequiredKeepingTracColumns = new List<string>() { "Estimate", "Station", "Air Date", "Air Time", "Air ISCI", "Demographic", "Act Ratings", "Act Impression" };

            //get the tab that needs processing
            var worksheet = _ExcelHelper.GetWorksheetToProcess(fileInfo, currentFile, _ValidStrataTabName);
            currentFile.Source = FileSourceEnum.Strata;

            //if (worksheet == null)
            //{
            //    worksheet = _ExcelHelper.GetWorksheetToProcess(fileInfo, currentFile, _ValidKeepingTracTabName);
            //    currentFile.Source = FileSourceEnum.KeepingTrac;
            //}
            if (worksheet == null)
            {
                currentFile.Source = FileSourceEnum.Unknown;
                currentFile.ErrorMessages.Add(string.Format("Could not find the tab {0} in file {1}", _ValidStrataTabName, currentFile.FilePath));
                //currentFile.ErrorMessages.Add(string.Format("Could not find the tab {0} in file {1}", _ValidKeepingTracTabName, currentFile.FilePath));
                return;
            }

            //List<string> requiredColumns = currentFile.Source == FileSourceEnum.Strata ? RequiredStrataColumns : RequiredKeepingTracColumns;

            //check column headers
            var fileColumns = _ExcelHelper.ValidateHeaders(RequiredStrataColumns, worksheet, currentFile);
            if (currentFile.ErrorMessages.Any())
                return;

            //check required data fields
            _ExcelHelper.CheckMissingDataOnRequiredColumns(RequiredStrataColumns, fileColumns, worksheet, currentFile);
            if (currentFile.ErrorMessages.Any())
                return;
        }
    }
}
