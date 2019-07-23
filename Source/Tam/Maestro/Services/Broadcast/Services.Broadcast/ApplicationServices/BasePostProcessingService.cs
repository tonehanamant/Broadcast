using Common.Services;
using Common.Services.ApplicationServices;
using Newtonsoft.Json;
using Services.Broadcast.ApplicationServices.Helpers;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using Tam.Maestro.Common;
using Tam.Maestro.Common.SystemComponentParameter;
using Tam.Maestro.Services.Cable.SystemComponentParameters;
using Tam.Maestro.Services.Clients;
using Tam.Maestro.Services.ContractInterfaces;

namespace Services.Broadcast.ApplicationServices
{
    public class BasePostProcessingService
    {
        private readonly IBroadcastAudiencesCache _AudienceCache;
        private readonly IWWTVFtpHelper _WWTVFtpHelper;
        private readonly IFileTransferEmailHelper _EmailHelper;
        private readonly IEmailerService _EmailerService;
        private readonly IFileService _FileService;
        private readonly IDataLakeFileService _DataLakeFileService;

        private const string VALID_INCOMING_FILE_EXTENSION = ".txt";

        public BasePostProcessingService(IFileTransferEmailHelper emailHelper
            , IWWTVFtpHelper wwtvFTPHelper
            , IBroadcastAudiencesCache audienceCache
            , IEmailerService emailerService
            , IFileService fileService
            , IDataLakeFileService dataLakeFileService)
        {
            _EmailHelper = emailHelper;
            _WWTVFtpHelper = wwtvFTPHelper;
            _AudienceCache = audienceCache;
            _EmailerService = emailerService;
            _FileService = fileService;
            _DataLakeFileService = dataLakeFileService;
        }

        /// <summary>
        /// Downloads a file list that needs to be processed
        /// </summary>
        /// <param name="path">Path to FTP directory containing the files</param>
        /// <returns>List of ftp file paths</returns>
        public List<string> DownloadFilesToBeProcessed(string path)
        {
            var environment = MaestroEnvironmentSystemParameterNames.Environment;

            try
            {
                return _WWTVFtpHelper.GetInboundFileList(path, (file) => file.EndsWith(VALID_INCOMING_FILE_EXTENSION));
            }
            catch (Exception e)
            {
                var emailBody =
                "There was an error reading from or connecting to the FTP server. \nHere is some technical information: \n\n" + e;
                _EmailHelper.SendEmail(emailBody, "WWTV FTP Error - " + environment);
                throw;
            }
        }
        
        /// <summary>
        /// Process a WWTV post processing file
        /// </summary>
        /// <param name="filePath">Path of the file to process</param>
        /// <returns>InboundFileSaveRequest object </returns>
        public InboundFileSaveRequest ParseWWTVFile(string fileName, string fileContents, List<WWTVInboundFileValidationResult> validationErrors)
        {
            InboundFileSaveRequest saveRequest = new InboundFileSaveRequest();
            try
            {
                saveRequest.FileName = Path.GetFileName(fileName);
                saveRequest.FileHash = HashGenerator.ComputeHash(fileContents.ToByteArray());

                if (!Path.GetExtension(fileName).Equals(VALID_INCOMING_FILE_EXTENSION))
                {
                    throw new Exception("Invalid file extension.");
                }
                _MapWWTVFileToInboundFileSaveRequest(fileContents, saveRequest, validationErrors);
            }
            catch (Exception e)
            {
                validationErrors.Add(new WWTVInboundFileValidationResult()
                {
                    ErrorMessage = "Could not process file.\n  " + e.ToString()
                });
                return saveRequest;
            }

            return saveRequest;
        }

        public void EmailFTPErrorFiles(List<string> filePaths)
        {
            if (!BroadcastServiceSystemParameter.EmailNotificationsEnabled)
                return;

            filePaths.ForEach(filePath =>
            {
                var body = string.Format("{0}", Path.GetFileName(filePath));
                var subject = "Error files from WWTV";
                var from = BroadcastServiceSystemParameter.EmailUsername;
                var Tos = new string[] { BroadcastServiceSystemParameter.WWTV_NotificationEmail };
                _EmailerService.QuickSend(true, body, subject, MailPriority.Normal, from, Tos, new List<string>() { filePath });
            });
        }

        public List<string> DownloadFTPFiles(List<string> files, string remoteFtpPath)
        {
            var localPaths = new List<string>();
            using (var ftpClient = _WWTVFtpHelper.EnsureFtpClient())
            {
                var localFolder = WWTVSharedNetworkHelper.GetLocalErrorFolder();
                files.ForEach(filePath =>
                {
                    var path = remoteFtpPath + "/" + filePath.Remove(0, filePath.IndexOf(@"/") + 1);
                    var localPath = localFolder + @"\" + filePath.Replace(@"/", @"\");
                    if (_FileService.Exists(localPath))
                        _FileService.Delete(localPath);
                    if (!_FileService.DirectoryExists(localPath))
                        _FileService.CreateDirectory(localPath);

                    _WWTVFtpHelper.DownloadFileFromClient(ftpClient, path, localPath);
                    localPaths.Add(localPath);
                    _WWTVFtpHelper.DeleteFiles(path);
                });
            }
            return localPaths;
        }
        
        /// <summary>
        /// Sends a file to data lake
        /// </summary>
        /// <param name="fileContents">File content to send</param>
        /// <param name="fileName">Filename for the file</param>
        public void SendFileToDataLake(string fileContents, string fileName)
        {
            try
            {
                _DataLakeFileService.Save(new FileRequest
                {
                    FileName = fileName,
                    StreamData = new MemoryStream(Encoding.UTF8.GetBytes(fileContents))
                });
            }
            catch
            {
                throw new ApplicationException("Unable to send WWTV file to Data Lake shared folder and e-mail reporting the error.");
            }
        }

        /// <summary>
        /// Deals with fact that time comes in 2 formats  HHMMTT and HMMTT 
        /// (single and double digit hour which is not supported properly by .NET library)
        /// </summary>
        private static TimeSpan ExtractStrataTime(string timeToParse, List<WWTVInboundFileValidationResult> validationErrors, string fieldName, int recordNumber)
        {
            Regex regExp = new Regex(@"^(?<hours>(([0][1-9]|[1][0-2]|[0-9])))(?<minutes>([0-5][0-9]))(?<ampm>A|P)$");
            var match = regExp.Match(timeToParse);

            if (!match.Success)
            {
                validationErrors.Add(new WWTVInboundFileValidationResult()
                {
                    ErrorMessage = "is invalid time.  Please use format \"HHMMA|P\".",
                    InvalidField = fieldName,
                    InvalidLine = recordNumber
                });
                return new TimeSpan();
            }

            DateTime result = new DateTime();

            int hour = Int32.Parse(match.Groups["hours"].Value);
            if (match.Groups["ampm"].Value == "P" && hour < 12)
                hour += 12;
            if (match.Groups["ampm"].Value == "A" && hour == 12)
                hour = 0;
            int minutes = Int32.Parse(match.Groups["minutes"].Value);
            
            result = new DateTime(1, 1, 1, hour, minutes, 0);
            return result.TimeOfDay;
        }

        private TimeSpan ExtractDateTime(string datetime, List<WWTVInboundFileValidationResult> validationErrors, string fieldName, int recordNumber)
        {
            if (!DateTime.TryParse(datetime, out DateTime parsedTime))
            {
                validationErrors.Add(new WWTVInboundFileValidationResult()
                {
                    ErrorMessage = "is invalid date or time.",
                    InvalidField = fieldName,
                    InvalidLine = recordNumber
                });
                return new TimeSpan();
            }

            return parsedTime.TimeOfDay;
        }

        private InboundFileSaveRequest _MapWWTVFileToInboundFileSaveRequest(string fileContents, InboundFileSaveRequest saveRequest, List<WWTVInboundFileValidationResult> validationErrors)
        {
            saveRequest.Details = new List<InboundFileSaveRequestDetail>();

            WhosWatchingTVPostProcessingFile jsonFile;
            try
            {
                jsonFile = new WhosWatchingTVPostProcessingFile
                {
                    Details = JsonConvert.DeserializeObject<List<WhosWatchingTVDetail>>(fileContents)
                };
            }
            catch (Exception e)
            {
                validationErrors.Add(new WWTVInboundFileValidationResult()
                {
                    ErrorMessage = "File is in an invalid format.  It cannot be read in its current state; must be a valid JSON file." +
                                   "\r\n" + e.ToString()
                });
                return saveRequest;
            }

            for (var recordNumber = 0; recordNumber < jsonFile.Details.Count; recordNumber++)
            {
                var jsonDetail = jsonFile.Details[recordNumber];

                if (!Enum.TryParse(jsonDetail.InventorySource, out DeliveryFileSourceEnum inventorySource))
                {
                    validationErrors.Add(new WWTVInboundFileValidationResult()
                    {
                        ErrorMessage = "is invalid (" + jsonDetail.InventorySource + ")",
                        InvalidField = "InventorySource",
                        InvalidLine = recordNumber
                    });
                }
                else
                {
                    saveRequest.Source = (int)inventorySource;
                }

                DateTime airTime = DateTime.Now;

                if (saveRequest.Source == (int)DeliveryFileSourceEnum.Strata)
                {
                    airTime = jsonDetail.Date.Add(ExtractStrataTime(jsonDetail.Time, validationErrors, "Time", recordNumber));
                }
                else if (saveRequest.Source == (int)DeliveryFileSourceEnum.KeepingTrac)
                {
                    airTime = jsonDetail.Date.Add(ExtractDateTime(jsonDetail.Time, validationErrors, "Time", recordNumber));
                }
                var leadInEndTime = string.IsNullOrWhiteSpace(jsonDetail.LeadInEndTime) 
                    ? (DateTime?)null
                    :jsonDetail.Date.Add(ExtractDateTime(jsonDetail.LeadInEndTime, validationErrors, "LeadInEndTime", recordNumber));
                var leadOutStartTime = string.IsNullOrWhiteSpace(jsonDetail.LeadOutStartTime)
                    ? (DateTime?)null
                    : jsonDetail.Date.Add(ExtractDateTime(jsonDetail.LeadOutStartTime, validationErrors, "LeadOutStartTime", recordNumber));

                var saveRequestDetail = new InboundFileSaveRequestDetail()
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
                    LeadOutShowType = jsonDetail.LeadOutShowType,
                    SuppliedProgramName = jsonDetail.SuppliedProgram
                };

                if (jsonDetail.Demographics != null)
                {
                    saveRequestDetail.Demographics = jsonDetail.Demographics.Select(y =>
                    {
                        if (!_IsValidDemo(validationErrors, y, recordNumber, inventorySource, out string xtransformedCode))
                            return null;
                        var audienceId = _AudienceCache.GetDisplayAudienceByCode(xtransformedCode).Id;

                        return new ScrubbingDemographics()
                        {
                            AudienceId = audienceId,
                            OvernightImpressions = y.OvernightImpressions,
                            OvernightRating = y.OvernightRating
                        };
                    }).ToList();

                }

                saveRequest.Details.Add(saveRequestDetail);
            }
            return saveRequest;
        }

        private bool _IsValidDemo(List<WWTVInboundFileValidationResult> validationErrors, ScrubbingDemographics demo, int recordNumber
            , DeliveryFileSourceEnum inventorySource, out string xtransformedCode)
        {
            var audience = _AudienceCache.GetBroadcastAudienceByCode(demo.Demographic);

            if (audience == null)
            {
                validationErrors.Add(new WWTVInboundFileValidationResult()
                {
                    ErrorMessage = "is invalid demo/audience code," + demo.Demographic + ".",
                    InvalidField = "Demographic Code",
                    InvalidLine = recordNumber
                });
                xtransformedCode = null;
                return false;
            }
            xtransformedCode = audience.Code;

            return true;
        }
    }
}
