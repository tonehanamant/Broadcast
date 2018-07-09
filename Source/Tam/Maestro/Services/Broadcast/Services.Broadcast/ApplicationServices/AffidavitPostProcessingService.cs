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
    public class DownloadAndProcessWWTVFilesResponse
    {
        public List<string> FilesFoundToProcess { get; set; } = new List<string>();
        public List<string> FailedDownloads { get; set; } = new List<string>();
        public Dictionary<string, List<AffidavitValidationResult>> ValidationErrors { get; set; } = new Dictionary<string, List<AffidavitValidationResult>>();
        public List<AffidavitSaveResult> AffidavitSaveResults { get; set; } = new List<AffidavitSaveResult>();
    }



    public interface IAffidavitPostProcessingService : IApplicationService
    {
        /// <summary>
        /// Downloads the WWTV processed files and calls the affidavit processing service
        /// </summary>
        DownloadAndProcessWWTVFilesResponse DownloadAndProcessWWTVFiles(string userName);

        AffidavitSaveResult ProcessFileContents(string userName, string fileName, string fileContents);
    }


    public class AffidavitPostProcessingService : IAffidavitPostProcessingService
    {
        private readonly IBroadcastAudiencesCache _AudienceCache;
        private readonly IAffidavitEmailProcessorService _affidavitEmailProcessorService;
        private readonly IAffidavitService _AffidavidService;
        private readonly IWWTVFtpHelper _WWTVFtpHelper;

        private const string VALID_INCOMING_FILE_EXTENSION = ".txt";


        public AffidavitPostProcessingService(
            IBroadcastAudiencesCache audienceCache,
            IDataRepositoryFactory broadcastDataRepositoryFactory,
            IAffidavitEmailProcessorService affidavitEmailProcessorService,
            IAffidavitService affidavidService,
            IWWTVFtpHelper ftpHelper)
        {
            _AudienceCache = audienceCache;
            _affidavitEmailProcessorService = affidavitEmailProcessorService;
            _AffidavidService = affidavidService;
            _WWTVFtpHelper = ftpHelper;
        }


        /// <summary>
        /// Downloads the WWTV processed files and calls the affidavit processing service
        /// return true if download success, false if download fails (use for loggin)
        /// This involves FTP 
        /// </summary>
        public DownloadAndProcessWWTVFilesResponse DownloadAndProcessWWTVFiles(string userName)
        {
            List<string> filesToProcess;
            var response = new DownloadAndProcessWWTVFilesResponse();

            try
            {
                filesToProcess = _GetWWTVFTPFileNames();
            }
            catch (Exception e)
            {
                _ProceseTotalFTPFailure(e);
                throw;
            }

            response.FilesFoundToProcess.AddRange(filesToProcess);
            if (!filesToProcess.Any())
            {
                return response;
            }

            var inboundFtpPath = _WWTVFtpHelper.GetInboundPath();

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
                    response.FailedDownloads.AddRange(failedDownloads);
                    continue; // skip to next file 
                }
                string fileName = Path.GetFileName(filePath);

                var ftpFileToDelete = inboundFtpPath + "/" + fileName;
                try
                {
                    _WWTVFtpHelper.DeleteFile(ftpFileToDelete);
                }
                catch (Exception e)
                {
                    var errorMessage = "Error deleting affidavit file from FTP site: " + ftpFileToDelete + "\r\n" + e;
                    _affidavitEmailProcessorService.ProcessAndSendTechError(filePath, errorMessage,fileContents);
                    continue;
                }

                var result = ProcessFileContents(userName, fileName, fileContents);
                response.AffidavitSaveResults.Add(result);
                if (result.ValidationResults.Any())
                {
                    _affidavitEmailProcessorService.ProcessAndSendValidationErrors(fileName, result.ValidationResults,
                        fileContents);

                    response.ValidationErrors.Add(fileName, result.ValidationResults);
                }
            }

            _affidavitEmailProcessorService.ProcessAndSendFailedFiles(failedDownloads,inboundFtpPath);

            return response;
        }

        public AffidavitSaveResult ProcessFileContents(string userName, string fileName, string fileContents)
        {
            List<AffidavitValidationResult> validationErrors = new List<AffidavitValidationResult>();
            AffidavitSaveRequest affidavitSaveRequest = ParseWWTVFile(fileName, fileContents, validationErrors);
            if (validationErrors.Any())
            {
                return _AffidavidService.SaveAffidavitValidationErrors(affidavitSaveRequest, userName, validationErrors);
            }

            AffidavitSaveResult result;
            try
            {
                result = _AffidavidService.SaveAffidavit(affidavitSaveRequest, userName, DateTime.Now);
            }
            catch (Exception e)
            {
                string errorMessage = "Error saving affidavit:\n\n" + e.ToString();
                _affidavitEmailProcessorService.ProcessAndSendTechError(fileName, errorMessage,fileContents);
                return null;
            }

            return result;
        }


    
        private void _ProceseTotalFTPFailure(Exception e)
        {
            var emailBody =
                "There was an error reading from or connecting to the FTP server. \n\nHere is some technical information." + e;
            _affidavitEmailProcessorService.Send(emailBody,"WWTV FTP Error");
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
                affidavitSaveRequest.FileName = Path.GetFileName(fileName);
                affidavitSaveRequest.FileHash = HashGenerator.ComputeHash(fileContents.ToByteArray());

                if (!Path.GetExtension(fileName).Equals(VALID_INCOMING_FILE_EXTENSION))
                {
                    throw new Exception("Invalid file extension.");
                }
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

            return affidavitSaveRequest;
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
                        ErrorMessage = "is invalid (" + jsonDetail.InventorySource + ")",
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
            return _WWTVFtpHelper.DownloadFileFtpToString(fileName);

        }


        private List<string> _GetWWTVFTPFileNames()
        {
            return _WWTVFtpHelper.GetInboundFileList((file) => file.EndsWith(VALID_INCOMING_FILE_EXTENSION));
        }
    }
}