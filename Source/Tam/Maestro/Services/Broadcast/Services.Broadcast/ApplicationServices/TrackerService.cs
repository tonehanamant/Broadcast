﻿using Common.Services;
using Common.Services.ApplicationServices;
using Common.Services.Extensions;
using Common.Services.Repositories;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
using Services.Broadcast.Converters;
using Services.Broadcast.Converters.Scx;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.DTO;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Tam.Maestro.Common;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.ApplicationServices
{
    public interface ITrackerService : IApplicationService
    {
        LoadSchedulesDto GetSchedulesByDate(DateTime? startDate, DateTime? endDate);
        DetectionLoadDto GetDetectionLoadData(DateTime currentDateTime);
        int SaveSchedule(ScheduleSaveRequest request);
        Tuple<List<int>, string> SaveDetectionFiles(FileSaveRequest request, string username, bool isSigmaUpload = false);
        string SaveDetectionFileViaFtp(string userName);
        bool ScheduleExists(int estimateIds);
        DetectionScrubbingDto GetDetectionScrubbingData(int estimateId);
        List<LookupDto> GetSchedulePrograms(int scheduleId);
        List<LookupDto> GetScheduleStations(int scheduleId);
        List<DetectionTrackingDetail> SaveScrubbingMapping(ScrubbingMap map);
        List<ScheduleDetail> GetScheduleDetailsByEstimateId(int estimateId);
        Dictionary<int, int> GetScheduleAudiences(int estimateId);
        bool ScrubSchedule(ScheduleScrubbing scheduleScrubbing);
        DetectionMap GetDetectionMapByType(string mappingType);
        bool DeleteMapping(string mappingType, TrackingMapValue mapping);
        ScheduleHeaderDto GetScheduleHeader(int estimateId);
        DisplaySchedule GetDisplayScheduleById(int scheduleId);
        bool UpdateRatingAdjustments(List<RatingAdjustmentsDto> ratingAdjustments);
        RatingAdjustmentsResponse GetRatingAdjustments();
        List<DetectionFileSummary> GetDetectionFileSummaries();
        bool TrackSchedule(int scheduleId);
        bool DeleteDetectionFile(int detectionFileId);
        List<DisplaySchedule> GetDisplaySchedulesWithAdjustedImpressions(DateTime? startDate, DateTime? endDate);
        List<DetectionTrackingDetail> GetDetectionDetailsWithAdjustedImpressions(int estimateId, ScheduleDTO schedule);       
    }

    public class TrackerService : BroadcastBaseClass, ITrackerService
    {
        private const int MaxFTPFailuresAllow = 1; // 1 for now, maybe more in the future

        private readonly IDataRepositoryFactory _BroadcastDataRepositoryFactory;
        private readonly IDetectionPostingEngine _DetectionPostingEngine;
        private readonly ITrackingEngine _TrackingEngine;
        private readonly IScxScheduleConverter _ScxConverter;
        private readonly IDetectionConverter _DetectionConverter;
        private readonly ISigmaConverter _SigmaConverter;
        private readonly IAssemblyScheduleConverter _AssemblyFileConverter;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekAggregateCache;
        private readonly IBroadcastAudiencesCache _AudiencesCache;
        private readonly IDefaultScheduleConverter _DefaultScheduleConverter;
        private readonly IDaypartCache _DayPartCache;
        private readonly IQuarterCalculationEngine _QuarterCalculationEngine;
        private readonly IImpressionAdjustmentEngine _ImpressionAdjustmentEngine;
        private readonly INsiPostingBookService _NsiPostingBookService;
        private readonly IFileService _FileService;
        private readonly IProposalRepository _ProposalRepository;
        private readonly IAabEngine _AabEngine;

        private readonly Lazy<string> _FtpDirectory;
        private readonly Lazy<string> _FtpSaveFolder;
        private readonly Lazy<string> _FtpUrl;
        private readonly Lazy<string> _FtpUserName;
        private readonly Lazy<string> _FtpPassword;

        public TrackerService(IDataRepositoryFactory broadcastDataRepositoryFactory
            , IDetectionPostingEngine detectionPostingEngine
            , ITrackingEngine trackingEngine
            , IScxScheduleConverter scxConverter
            , IDetectionConverter detectionConverter
            , ISigmaConverter sigmaConverter
            , IAssemblyScheduleConverter assemblyFileConverter
            , IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache
            , IBroadcastAudiencesCache audiencesCache, IDefaultScheduleConverter defaultScheduleConverter
            , IDaypartCache dayPartCache, IQuarterCalculationEngine quarterCalculationEngine
            , IAabEngine aabEngine
            , IImpressionAdjustmentEngine impressionAdjustmentEngine
            , INsiPostingBookService nsiPostingBookService
            , IFileService fileService, IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper) : base(featureToggleHelper, configurationSettingsHelper)
        {
            _BroadcastDataRepositoryFactory = broadcastDataRepositoryFactory;
            _DetectionPostingEngine = detectionPostingEngine;
            _TrackingEngine = trackingEngine;
            _ScxConverter = scxConverter;
            _DetectionConverter = detectionConverter;
            _SigmaConverter = sigmaConverter;
            _AssemblyFileConverter = assemblyFileConverter;
            _MediaMonthAndWeekAggregateCache = mediaMonthAndWeekAggregateCache;
            _AudiencesCache = audiencesCache;
            _DefaultScheduleConverter = defaultScheduleConverter;
            _DayPartCache = dayPartCache;
            _QuarterCalculationEngine = quarterCalculationEngine;
            _AabEngine = aabEngine;
            _ImpressionAdjustmentEngine = impressionAdjustmentEngine;
            _NsiPostingBookService = nsiPostingBookService;
            _FileService = fileService;
            _ProposalRepository = _BroadcastDataRepositoryFactory.GetDataRepository<IProposalRepository>();

            _FtpDirectory = new Lazy<string>(_GetFtpDirectory);
            _FtpSaveFolder = new Lazy<string>(_GetFtpSaveFolder);
            _FtpUrl = new Lazy<string>(_GetFtpUrl);
            _FtpUserName = new Lazy<string>(_GetFtpUserName);
            _FtpPassword = new Lazy<string>(_GetFtpPassword);
        }

        private string _GetFtpDirectory()
        {
            var result = _ConfigurationSettingsHelper.GetConfigValueWithDefault<string>(ConfigKeys.TrackerServiceFtpDirectory, string.Empty);
            return result;
        }

        private string _GetFtpSaveFolder()
        {
            var result = _ConfigurationSettingsHelper.GetConfigValueWithDefault<string>(ConfigKeys.TrackerServiceFtpSaveFolder, string.Empty);
            return result;
        }

        private string _GetFtpUrl()
        {
            var result = _ConfigurationSettingsHelper.GetConfigValueWithDefault<string>(ConfigKeys.TrackerServiceFtpUrl, string.Empty);
            return result;
        }

        private string _GetFtpUserName()
        {
            var result = _ConfigurationSettingsHelper.GetConfigValueWithDefault<string>(ConfigKeys.TrackerServiceFtpUserName, string.Empty);
            return result;
        }

        private string _GetFtpPassword()
        {
            var rawResult = _ConfigurationSettingsHelper.GetConfigValueWithDefault<string>(ConfigKeys.TrackerServiceFtpPassword, string.Empty);
            var result = EncryptionHelper.DecryptString(rawResult, EncryptionHelper.EncryptionKey);
            return result;
        }

        public LoadSchedulesDto GetSchedulesByDate(DateTime? startDate, DateTime? endDate)
        {
            var ret = new LoadSchedulesDto();

            ret.Schedules = GetDisplaySchedulesWithAdjustedImpressions(startDate, endDate);

            var scheduleMasterAdvertisers = ret.Schedules.Select(x => x.AdvertiserMasterId).ToList();

            var nsiPostingBooks = _NsiPostingBookService.GetNsiPostingMediaMonths();
            ret.PostingBooks = nsiPostingBooks.Select(d => new LookupDto() { Id = d.Id, Display = d.MediaMonthX }).ToList();
                var AabAdvertisers = _AabEngine.GetAdvertisers().Select(_MapToLoadSchedulesDto).ToList();
                ret.AabAdvertisers = AabAdvertisers.Where(a => scheduleMasterAdvertisers.Contains(a.Id)).ToList();
                foreach (var schedule in ret.Schedules)
                {
                    var aabadvertiser = ret.AabAdvertisers.FirstOrDefault(a => a.Id == schedule.AdvertiserMasterId);
                    var postingBook = nsiPostingBooks.FirstOrDefault(p => p.Id == schedule.PostingBookId);

                    schedule.Advertiser = aabadvertiser == null ? "" : aabadvertiser.Display;
                    schedule.PostingBook = postingBook == null ? "" : postingBook.MediaMonthX;
                    schedule.PostingBookDate = postingBook == null ? (DateTime?)null : postingBook.EndDate;
                }
            return ret;
        }

        public List<DisplaySchedule> GetDisplaySchedulesWithAdjustedImpressions(DateTime? startDate, DateTime? endDate)
        {
            var displaySchedules = _BroadcastDataRepositoryFactory.GetDataRepository<IScheduleRepository>().GetDisplaySchedules(startDate, endDate);

            foreach (var schedule in displaySchedules)
            {
                foreach (var trackingDetails in schedule.DeliveryDetails)
                {
                    if (trackingDetails.Impressions != null)
                        schedule.PrimaryDemoDelivered += _ImpressionAdjustmentEngine.AdjustImpression(trackingDetails.Impressions.Value, schedule.IsEquivalized, trackingDetails.SpotLength, schedule.PostType, schedule.PostingBookId);
                }
            }

            return displaySchedules;
        }

        public DetectionLoadDto GetDetectionLoadData(DateTime currentDateTime)
        {
            var ret = new DetectionLoadDto
            {
                Quarters = _GetQuarters(),
                PostingBooks = _NsiPostingBookService.GetNsiPostingBookMonths(),
                InventorySources = Enum.GetValues(typeof(InventorySourceEnum))
                    .Cast<InventorySourceEnum>()
                    .Where(e => e != InventorySourceEnum.Blank)
                    .Select(e => new LookupDto { Display = e.Description(), Id = (int)e })
                    .ToList(),
                SchedulePostTypes = EnumExtensions.ToLookupDtoList<PostingTypeEnum>(),
                Markets = _BroadcastDataRepositoryFactory.GetDataRepository<IMarketRepository>()
                    .GetMarkets()
                    .Select(m => new LookupDto { Display = m.geography_name, Id = m.market_code })
                    .ToList(),
                Audiences = _AudiencesCache.GetAllLookups()
            };
            ret.AabAdvertisers = _AabEngine.GetAdvertisers().Select(_MapToLoadSchedulesDto).ToList();
            ret.CurrentQuarter = ret.Quarters.Single(x => x.StartDate <= currentDateTime && x.EndDate >= currentDateTime);

            return ret;
        }

        private List<Quarter> _GetQuarters()
        {
            var years = new List<int>();

            var maxEndDate = _BroadcastDataRepositoryFactory.GetDataRepository<IScheduleRepository>().GetMaxEndDate();
            var startYear = 2014;
            if (maxEndDate.Year < startYear)
            {
                startYear = maxEndDate.Year;
            }

            for (; startYear <= Math.Max(DateTime.Now.Year, maxEndDate.Year); startYear++)
            {
                years.Add(startYear);
            }

            var qtrCtr = 0;
            var baseDateTime = DateTime.Parse("1/1/2014");
            var qtrRange = _QuarterCalculationEngine.GetQuarterRangeByDate(baseDateTime, qtrCtr);

            var ret = new List<Quarter>();
            foreach (var year in years)
            {
                for (var qtr = 1; qtr <= 4; qtr++)
                {
                    var quarter = new Quarter
                    {
                        Id = qtrCtr + 1,
                        EndDate = qtrRange.EndDate,
                        StartDate = qtrRange.StartDate,
                        Display = year + "Q" + qtr,
                    };
                    ret.Add(quarter);
                    qtrCtr++;
                    qtrRange = _QuarterCalculationEngine.GetQuarterRangeByDate(baseDateTime, qtrCtr);
                }
            }
            return ret;
        }

        public ScheduleFileType _GetRequestFileType(ScheduleSaveRequest request)
        {
            if (request.Schedule.IsBlank || request.Schedule.FileName == null)
            {
                return ScheduleFileType.Default;
            }

            var extIndex = request.Schedule.FileName.LastIndexOf(".");
            if (extIndex < 0)
                throw new Exception("Could not determine file type.");

            var ext = request.Schedule.FileName.Substring(extIndex + 1).ToLower();
            if (string.IsNullOrEmpty(ext))
                throw new Exception("Could not determine file type.");

            if (ext == "scx")
                return ScheduleFileType.Scx;
            if (ext == "csv")
                return ScheduleFileType.Csv;

            throw new Exception("Could not determine file type.");
        }

        private IScheduleConverter GetScheduleConverter(ScheduleSaveRequest request)
        {
            var requestType = _GetRequestFileType(request);

            IScheduleConverter converter;
            switch (requestType)
            {
                case ScheduleFileType.Scx:
                    converter = _ScxConverter;
                    break;

                case ScheduleFileType.Csv:
                    converter = _AssemblyFileConverter;
                    break;

                default:
                    converter = _DefaultScheduleConverter;
                    break;
            }
            return converter;
        }

        public int SaveSchedule(ScheduleSaveRequest request)
        {
            var advertiserId = _AabEngine.GetAdvertisers().Where(x => x.MasterId == request.Schedule.AdvertiserMasterId).ToList().Select(a => a.Id).FirstOrDefault();
            request.Schedule.AdvertiserId = advertiserId == null ? 0 : (int)advertiserId;
            var converter = GetScheduleConverter(request);
            var scheduleDto = request.Schedule;

            _ValidateScheduleIscis(scheduleDto.ISCIs);

            var efSchedule = converter.Convert(scheduleDto);

            int scheduleId;
            var broadcastRepo = _BroadcastDataRepositoryFactory.GetDataRepository<IScheduleRepository>();

            var creating = scheduleDto.EstimateId != null && scheduleDto.FileStream != null || scheduleDto.IsBlank;
            if (creating)
            {
                _DeleteExistingSchedule(scheduleDto, broadcastRepo);
                scheduleId = broadcastRepo.SaveSchedule(efSchedule);
            }
            else
            {
                broadcastRepo.ClearScheduleAudiences(efSchedule.id);
                broadcastRepo.ClearScheduleIscis(efSchedule.id);
                broadcastRepo.ClearScheduleMarketRestrictions(efSchedule.id);
                broadcastRepo.ClearScheduleDaypartRestrictions(efSchedule.id);

                broadcastRepo.UpdateSchedule(efSchedule);
                broadcastRepo.UpdateScheduleMarketRestrictions(efSchedule.id, scheduleDto.MarketRestrictions);

                scheduleId = efSchedule.id;
            }

            //there might be existing bvs data that links to a new schedule being saved now
            //the demos may have also changed in an updated schedule
            //we need to re-post the bvs data for this estimate id
            if (scheduleDto.EstimateId != null)
            {
                _DetectionPostingEngine.PostDetectionDataByEstimate((int)scheduleDto.EstimateId);
                _TrackingEngine.TrackDetectionByEstimateId((int)scheduleDto.EstimateId);
            }
            else if (converter is IDefaultScheduleConverter)
            {
                var iscis = efSchedule.schedule_iscis.Select(si => si.house_isci).ToList();
                var scheduleAudiences = efSchedule.schedule_audiences.ToDictionary(sa => sa.rank, sa => sa.audience_id);
                _DetectionPostingEngine.PostDetectionData(iscis, scheduleAudiences, scheduleDto.PostingBookId);
            }

            return scheduleId;
        }

        // conditions separated for proper error messages
        private static void _ValidateScheduleIscis(List<IsciDto> iscis)
        {
            if (iscis == null)
            {
                throw new ApplicationException("No ISCIs informed");
            }

            if (iscis.Any(i => string.IsNullOrEmpty(i.House)))
            {
                throw new ApplicationException("All ISCIs must have a house ISCI");
            }

            if (iscis.Any(i => string.IsNullOrEmpty(i.Client)))
            {
                throw new ApplicationException("All ISCIs must have a client ISCI");
            }
        }

        private void _DeleteExistingSchedule(ScheduleDTO schedule, IScheduleRepository broadcastRepo)
        {
            //if the schedule exists, delete it
            if (schedule.EstimateId.HasValue && ScheduleExists(schedule.EstimateId.Value))
            {
                // update BVS detail records' schedule_detail_week_id column to null for the schedule's estimate ID prior to deleting the schedule data
                var bvsRepo = _BroadcastDataRepositoryFactory.GetDataRepository<IDetectionRepository>();
                bvsRepo.ClearTrackingDetailsByEstimateId(schedule.EstimateId.Value);
                broadcastRepo.DeleteSchedule(schedule.EstimateId.Value);
            }
            else if (schedule.Id != 0)
            {
                broadcastRepo.DeleteScheduleById(schedule.Id);
            }
        }

        public bool ScrubSchedule(ScheduleScrubbing scheduleScrubbing)
        {
            // Update the bvsDetails as 'Officialy Out of Spec'
            if (scheduleScrubbing.OfficiallyOutOfSpecIds != null && scheduleScrubbing.OfficiallyOutOfSpecIds.Count > 0)
            {
                var bvsRepo = _BroadcastDataRepositoryFactory.GetDataRepository<IDetectionRepository>();
                var officiallyOutOfSpecBvsItems = bvsRepo.GetDetectionTrackingDetailsByDetailIds(scheduleScrubbing.OfficiallyOutOfSpecIds);
                officiallyOutOfSpecBvsItems.ForEach(c => c.Status = TrackingStatus.OfficialOutOfSpec);
                bvsRepo.PersistDetectionDetails(officiallyOutOfSpecBvsItems);
            }

            var scheduleRepo = _BroadcastDataRepositoryFactory.GetDataRepository<IScheduleRepository>();
            var efSchedule = scheduleRepo.FindByEstimateId(scheduleScrubbing.EstimateId);

            var postingBookUpdated = scheduleScrubbing.PostingBookId != efSchedule.posting_book_id;
            if (efSchedule.estimate_id != null && postingBookUpdated)
            {
                scheduleRepo.UpdateSchedulePostingBook(efSchedule.id, scheduleScrubbing.PostingBookId);

                // Reevaluates the RatingsAggregates (delivery) for each station
                _DetectionPostingEngine.PostDetectionDataByEstimate((int)efSchedule.estimate_id);
            }

            return true;
        }

        /// <summary>
        /// Returns bvsID.
        /// </summary>
        public Tuple<List<int>, string> SaveDetectionFiles(FileSaveRequest request, string username, bool isSigmaUpload = false)
        {
            var duplicateMessage = string.Empty;

            var bvsIds = new List<int>();
            var bvsRepo = _BroadcastDataRepositoryFactory.GetDataRepository<IDetectionRepository>();
            var errorMessage = string.Empty;
            var hasErrors = false;

            var txId = Guid.NewGuid();
            var totalFileCount = request.Files.Count;
            var currentFileIndex = 0;

            _LogInfo($"Starting request to process {totalFileCount} files.  IsSigmaUpload = '{isSigmaUpload}'", txId, username);

            foreach (var requestDetectionFile in request.Files)
            {
                currentFileIndex++;
                _LogInfo($"Beginning file {currentFileIndex} of {totalFileCount}.  FileName = '{requestDetectionFile.FileName}'; FileLength = '{requestDetectionFile?.RawData?.Length ?? 0}'", txId, username);
                try
                {
                    errorMessage += string.Format("Starting upload for file '{0}' . . .",
                        requestDetectionFile.FileName);

                    //compute file hash to check against duplicate files being loaded
                    var hash = HashGenerator.ComputeHash(StreamHelper.ReadToEnd(requestDetectionFile.StreamData));

                    //check if file has already been loaded
                    if (bvsRepo.GetDetectionFileIdByHash(hash) > 0)
                    {
                        throw new ApplicationException(
                            "Unable to load post log file. The selected post log file has already been loaded.");
                    }

                    //we made it this far, it must be a new file - persist the file
                    string message = string.Empty;
                    TrackerFile<DetectionFileDetail> detectionFile = new TrackerFile<DetectionFileDetail>();
                    Dictionary<TrackerFileDetailKey<DetectionFileDetail>, int> lineInfo =
                        new Dictionary<TrackerFileDetailKey<DetectionFileDetail>, int>();

                    _LogInfo($"Beginning file data extract...", txId, username);

                    if (isSigmaUpload)
                    {
                        detectionFile = _SigmaConverter.ExtractSigmaData(requestDetectionFile.StreamData, hash,
                            username, requestDetectionFile.FileName, out lineInfo);
                    }
                    else
                    {
                        detectionFile = _DetectionConverter.ExtractDetectionData(requestDetectionFile.StreamData, hash,
                            username, requestDetectionFile.FileName, out message, out lineInfo);
                    }

                    if (!string.IsNullOrEmpty(message))
                    {
                        errorMessage += "<br />" + message;
                        hasErrors = true;
                    }

                    _LogInfo($"Extracted {detectionFile.FileDetails.Count} detail items.", txId, username);
                    _LogInfo($"Beginning file data filter...", txId, username);

                    var filterResult = bvsRepo.FilterOutExistingDetails(detectionFile.FileDetails.ToList());
                    detectionFile.FileDetails = filterResult.New;

                    _LogInfo($"Filtered down to {detectionFile.FileDetails.Count} detail items. Updated {filterResult.Updated.Count}. Ignored {filterResult.Ignored.Count}.", txId, username);

                    if (filterResult.Ignored.Any())
                    {
                        duplicateMessage +=
                            "<p>The following line(s) were previously imported and were ignored:</p><ul>";
                        foreach (var file in filterResult.Ignored)
                        {
                            duplicateMessage += string.Format(
                                "<li>Line {0}: Station {1}, Date {2}, Time Aired {3}, ISCI {4}, Spot Length {5}, Campaign {6}, Advertiser {7}</li>",
                                lineInfo[new TrackerFileDetailKey<DetectionFileDetail>(file)], file.Station,
                                file.DateAired, file.TimeAired, file.Isci, file.SpotLength, file.EstimateId,
                                file.Advertiser);
                        }

                        duplicateMessage += "</ul>";
                    }

                    if (filterResult.Updated.Any())
                    {
                        duplicateMessage +=
                            "<p>The following line(s) were previously imported and were updated with new program name:</p><ul>";
                        foreach (var file in filterResult.Updated)
                        {
                            duplicateMessage += string.Format(
                                "<li>Line {0}: Station {1}, Date {2}, Time Aired {3}, ISCI {4}, Spot Length {5}, Campaign {6}, Advertiser {7}, Program Name {8}</li>",
                                lineInfo[new TrackerFileDetailKey<DetectionFileDetail>(file)], file.Station,
                                file.DateAired, file.TimeAired, file.Isci, file.SpotLength, file.EstimateId,
                                file.Advertiser, file.ProgramName);
                        }

                        duplicateMessage += "</ul>";
                    }

                    bvsIds.Add(bvsRepo.SaveDetectionFile(detectionFile));
                    errorMessage += string.Format("File '{0}' uploaded successfully.<br />",
                        requestDetectionFile.FileName);
                }
                catch (Exception e)
                {
                    hasErrors = true;
                    errorMessage +=
                        string.Format("<br /> Error processing file '{0}'. <br />Message:<br />{1}<br />{2}<br />",
                            requestDetectionFile.FileName, e.Message, e.StackTrace);

                    _LogError($"Error caught attempt to save file {currentFileIndex} of {totalFileCount}.  FileName = '{requestDetectionFile.FileName}'; FileLength = '{requestDetectionFile?.RawData?.Length ?? 0}'", txId, e, username);

                    var innerEx = e.InnerException;
                    var level = 1;
                    while (innerEx != null)
                    {
                        errorMessage += string.Format("\r\nInner Exception Details: {0}<br />{1}<br />",
                            innerEx.Message, innerEx.StackTrace);
                        _LogError($"Inner Exception Level {level}", innerEx);
                        level++;
                        innerEx = innerEx.InnerException;
                    }
                }
                finally
                {
                    _LogInfo($"Completed file {currentFileIndex} of {totalFileCount}.  FileName = '{requestDetectionFile.FileName}'; FileLength = '{requestDetectionFile?.RawData?.Length ?? 0}'", txId, username);
                }
            }

            _LogInfo($"Finalizing processing request of {totalFileCount} files.", txId, username);

            var estimateIds = _BroadcastDataRepositoryFactory.GetDataRepository<IDetectionRepository>().GetEstimateIdsWithSchedulesByFileIds(bvsIds);
            foreach (var estimateId in estimateIds)
            {
                //post for delivery
                _DetectionPostingEngine.PostDetectionDataByEstimate(estimateId);

                _TrackingEngine.TrackDetectionByEstimateId(estimateId);
            }

            _LogInfo($"Completed processing request of {totalFileCount} files.", txId, username);

            if (hasErrors)
                throw new ExtractDetectionException(errorMessage);

            return Tuple.Create(bvsIds, duplicateMessage);
        }

        /// <summary>
        ///  stream needs to be copied since it used twice, once to save the bvs file and again to save to network folder.
        /// </summary>
        private static MemoryStream CreateStreamCopy(Stream source)
        {
            var target = new MemoryStream();
            var buffer = new byte[8 * 1024];

            int size;
            do
            {
                size = source.Read(buffer, 0, 8 * 1024);
                target.Write(buffer, 0, size);
            } while (size > 0);
            source.Position = 0;

            return target;
        }

        public string SaveDetectionFileViaFtp(string userName)
        {
            var successFiles = string.Empty;
            var failMessages = string.Empty;
            var cableTvMessage = string.Empty;
            var ftpFailures = 0;
            IEnumerable<string> fileList;
            try
            {
                fileList = _GetFtpFilesAndNames();
            }
            catch (Exception)
            {
                failMessages += "Could not get initial list of files.  FTP server is probably down.";
                return failMessages;
            }

            foreach (var fileName in fileList)
            {
                MemoryStream stream = null;
                try
                {
                    try
                    {
                        stream = _GetFileStream(fileName);
                    }
                    catch (Exception e)
                    {
                        failMessages += string.Format("<br />FTP Error in downloading file: <i>{0}</i><br>Reason: {1}<br />", fileName, e.Message);
                        if (++ftpFailures >= MaxFTPFailuresAllow)
                        {
                            return PrepareReturnMessage(successFiles, failMessages, cableTvMessage);
                        }
                        continue;
                    }

                    // see CreateStreamCopy as to why
                    using (var streamCopy = CreateStreamCopy(stream))
                    {
                        try
                        {
                            var request = new FileSaveRequest();
                            request.Files.Add(new FileRequest { FileName = fileName, StreamData = stream });

                            //Requirements are to ignore this for FTP for now
                            SaveDetectionFiles(request, userName);

                            successFiles += fileName + Environment.NewLine;
                        }
                        catch (ExtractDetectionExceptionEmptyFiles)
                        {
                            failMessages += string.Format("<br />Empty File: <i>{0}</i><br />", fileName);
                            continue;
                        }
                        catch (ExtractDetectionExceptionCableTv)
                        {
                            cableTvMessage += string.Format("<br /><i>{0}</i><br />", fileName);
                            continue;
                        }
                        catch (Exception e)
                        {
                            failMessages += string.Format("<br />Error in file: <i>{0}</i><br />Reason: {1}<br />", fileName, e.Message);
                            continue;
                        }
                        _CleanupFile(streamCopy, fileName);
                    }
                }
                finally
                {
                    if (stream != null)
                        stream.Dispose();
                }
            }

            var fullMessage = PrepareReturnMessage(successFiles, failMessages, cableTvMessage);

            return fullMessage;
        }

        private static string PrepareReturnMessage(string successFiles, string failMessages, string cableTvMessage)
        {
            if (string.IsNullOrEmpty(successFiles))
                successFiles = "No files downloaded";

            var fullMessage = string.Format("<strong>Successful Files loaded:</strong><br />{0}<br />", successFiles);

            if (string.IsNullOrEmpty(failMessages))
                fullMessage += Environment.NewLine + "No errors";
            else
                fullMessage = string.Format("{0}<br /><strong>Error Files not loaded, review on ftp site:</strong><br />{1}<br />",
                                                fullMessage, failMessages);
            if (!string.IsNullOrEmpty(cableTvMessage))
                fullMessage = string.Format("{0}<br /><strong>Files containing Cable TV only records</strong><br />{1}<br />",
                                                fullMessage, cableTvMessage);

            return fullMessage;
        }

        private NetworkCredential _GetCredentials()
        {
            return new NetworkCredential(_FtpUserName.Value, _FtpPassword.Value);
        }

        private IEnumerable<string> _GetFtpFilesAndNames()
        {
            var fileList = new List<string>();

            // Get the object used to communicate with the server.
            var ftp = (FtpWebRequest)WebRequest.Create(_FtpUrl.Value + "/" +
                                                       _FtpDirectory.Value);

            ftp.Method = WebRequestMethods.Ftp.ListDirectory;
            ftp.Credentials = _GetCredentials();

            var response = (FtpWebResponse)ftp.GetResponse();
            var responseStream = response.GetResponseStream();

            var reader = new StreamReader(responseStream);
            while (!reader.EndOfStream)
            {
                var fileName = reader.ReadLine();
                if (fileName.EndsWith(".DAT"))
                {
                    fileList.Add(fileName.ToUpper());
                }
            }
            reader.Close();
            return fileList;
        }

        private MemoryStream _GetFileStream(string fileName)
        {
            var path = _FtpUrl.Value + "/" + _FtpDirectory.Value + "/" + fileName;
            using (var ftpClient = new WebClient())
            {
                ftpClient.Credentials = _GetCredentials();
                var stream = new MemoryStream(ftpClient.DownloadData(path));
                return stream;
            }
        }

        public void _CleanupFile(MemoryStream memoryStream, string fileName)
        {
            var path = _FtpSaveFolder.Value + "\\" + fileName;
            var fileStream = File.OpenWrite(path);
            memoryStream.WriteTo(fileStream);

            var fileUrl = _FtpUrl.Value + "/" + _FtpDirectory.Value + "/" + fileName;
            _DeleteFtpFile(fileUrl);
        }

        private void _DeleteFtpFile(string fileUrl)
        {
            var ftp = (FtpWebRequest)WebRequest.Create(fileUrl);

            ftp.Method = WebRequestMethods.Ftp.DeleteFile;
            ftp.Credentials = _GetCredentials();

            var response = (FtpWebResponse)ftp.GetResponse();
            response.Close();
        }

        public bool ScheduleExists(int estimateId)
        {
            return _BroadcastDataRepositoryFactory.GetDataRepository<IScheduleRepository>().ScheduleExists(estimateId);
        }

        public DetectionScrubbingDto GetDetectionScrubbingData(int estimateId)
        {
            var scrubbingDto = new DetectionScrubbingDto();

            var schedule = _BroadcastDataRepositoryFactory.GetDataRepository<IScheduleRepository>().GetScheduleDtoByEstimateId(estimateId);

            scrubbingDto.CurrentPostingBookId = schedule.PostingBookId;
            scrubbingDto.EstimateId = schedule.EstimateId;
            scrubbingDto.ScheduleName = schedule.ScheduleName;
            scrubbingDto.ISCIs = string.Join(",", schedule.ISCIs.Select(i => i.House));
            scrubbingDto.PostingBooks = _NsiPostingBookService.GetNsiPostingBookMonths();
            scrubbingDto.DetectionDetails = GetDetectionDetailsWithAdjustedImpressions(estimateId, schedule);
            scrubbingDto.SchedulePrograms = _BroadcastDataRepositoryFactory.GetDataRepository<IScheduleRepository>().GetScheduleLookupPrograms(schedule.Id);
            scrubbingDto.ScheduleNetworks = _BroadcastDataRepositoryFactory.GetDataRepository<IScheduleRepository>().GetScheduleLookupStations(schedule.Id);

            return scrubbingDto;
        }

        public List<DetectionTrackingDetail> GetDetectionDetailsWithAdjustedImpressions(int estimateId, ScheduleDTO schedule)
        {
            var details = _BroadcastDataRepositoryFactory.GetDataRepository<IDetectionRepository>().GetDetectionTrackingDetailsByEstimateId(estimateId);

            foreach (var detail in details)
            {
                if (detail.Impressions.HasValue)
                    detail.Impressions = _ImpressionAdjustmentEngine.AdjustImpression(detail.Impressions.Value, schedule.Equivalized, detail.SpotLength, schedule.PostType, schedule.PostingBookId);
            }

            return details;
        }

        public List<LookupDto> GetSchedulePrograms(int scheduleId)
        {
            var response = _BroadcastDataRepositoryFactory.GetDataRepository<IScheduleRepository>().GetScheduleLookupPrograms(scheduleId);

            return response;
        }

        public List<LookupDto> GetScheduleStations(int scheduleId)
        {
            var response = _BroadcastDataRepositoryFactory.GetDataRepository<IScheduleRepository>().GetScheduleLookupStations(scheduleId);

            return response;
        }

        public List<DetectionTrackingDetail> SaveScrubbingMapping(ScrubbingMap map)
        {
            TrackingMapType mapType;
            if (map.BvsProgram != null && map.ScheduleProgram != null)
            {
                mapType = TrackingMapType.Program;
            }
            else
            {
                mapType = TrackingMapType.Station;
            }
            _BroadcastDataRepositoryFactory.GetDataRepository<ITrackerMappingRepository>().SaveScrubbingMapping(map, mapType);
            _TrackingEngine.TrackDetectionByDetectionDetails(map.DetailIds, map.EstimateId);
            return _BroadcastDataRepositoryFactory.GetDataRepository<IDetectionRepository>().GetDetectionTrackingDetailsByDetailIds(map.DetailIds);
        }

        public Dictionary<int, int> GetScheduleAudiences(int estimateId)
        {
            return _BroadcastDataRepositoryFactory.GetDataRepository<IScheduleRepository>().GetDictionaryOfScheduleAudiencesByRank(estimateId);
        }

        public List<ScheduleDetail> GetScheduleDetailsByEstimateId(int estimateId)
        {
            return _BroadcastDataRepositoryFactory.GetDataRepository<IScheduleRepository>().GetScheduleTrackingDetails(estimateId);
        }

        public DetectionMap GetDetectionMapByType(string mappingTypeString)
        {
            var mapType = _ParseTrackingMapType(mappingTypeString);

            var bvsMap =
                _BroadcastDataRepositoryFactory.GetDataRepository<ITrackerMappingRepository>()
                    .GetMap((int)mapType);

            return bvsMap;
        }

        private static TrackingMapType _ParseTrackingMapType(string mappingTypeString)
        {
            TrackingMapType mapType;
            if (!Enum.TryParse(mappingTypeString, true, out mapType))
            {
                throw new ApplicationException(string.Format("Unknown type: {0}.", mappingTypeString));
            }
            return mapType;
        }

        public bool DeleteMapping(string mappingTypeString, TrackingMapValue mapping)
        {
            var mappingType = _ParseTrackingMapType(mappingTypeString);
            _BroadcastDataRepositoryFactory.GetDataRepository<ITrackerMappingRepository>()
                .DeleteMapping(mapping.DetectionValue, mapping.ScheduleValue, mappingType);
            return true;
        }

        public ScheduleHeaderDto GetScheduleHeader(int estimateId)
        {
            return _BroadcastDataRepositoryFactory.GetDataRepository<IScheduleRepository>().GetScheduleHeaderByEstimateId(estimateId);
        }

        public DisplaySchedule GetDisplayScheduleById(int scheduleId)
        {
            var schedule = _BroadcastDataRepositoryFactory.GetDataRepository<IScheduleRepository>().GetDisplayScheduleById(scheduleId);
            if (schedule.DaypartRestrictionId != 0)
            {
                schedule.DaypartRestriction = DaypartDto.ConvertDisplayDaypart(_DayPartCache.GetDisplayDaypart(schedule.DaypartRestrictionId));
            }

            return schedule;
        }

        public bool UpdateRatingAdjustments(List<RatingAdjustmentsDto> ratingAdjustments)
        {
            const decimal minPercentageValue = -100M;
            const decimal maxPercentageValue = 100M;
            if (ratingAdjustments.All(r => r.AnnualAdjustment >= minPercentageValue && r.AnnualAdjustment <= maxPercentageValue && r.NtiAdjustment >= minPercentageValue && r.NtiAdjustment <= maxPercentageValue))
            {
                _BroadcastDataRepositoryFactory.GetDataRepository<IRatingAdjustmentsRepository>().SaveRatingAdjustments(ratingAdjustments);
            }
            else
            {
                throw new ApplicationException("Percentage values must be between -100 and 100");
            }

            return true;
        }

        public RatingAdjustmentsResponse GetRatingAdjustments()
        {
            var adjustments = _BroadcastDataRepositoryFactory.GetDataRepository<IRatingAdjustmentsRepository>().GetRatingAdjustments();
            var postingBooks = _NsiPostingBookService.GetNsiPostingBookMonths();

            // update display for each adjustment
            adjustments.ForEach(a =>
            {
                var postingBook = postingBooks.FirstOrDefault(p => p.Id == a.MediaMonthId);
                if (postingBook != null)
                {
                    a.MediaMonthDisplay = postingBook.Display;
                }
            });

            var ratingAdjustments = new RatingAdjustmentsResponse
            {
                RatingAdjustments = adjustments,
                PostingBooks = postingBooks
            };

            return ratingAdjustments;
        }

        public List<DetectionFileSummary> GetDetectionFileSummaries()
        {
            return _BroadcastDataRepositoryFactory.GetDataRepository<IDetectionRepository>().GetDetectionFileSummaries();
        }

        public bool TrackSchedule(int scheduleId)
        {
            var scheduleRepo = _BroadcastDataRepositoryFactory.GetDataRepository<IScheduleRepository>();
            var scheduleDto = scheduleRepo.GetById(scheduleId);
            if (scheduleDto == null || scheduleDto.estimate_id == null)
                throw new Exception("Could not load schedule from Id=" + scheduleId);

            _DetectionPostingEngine.PostDetectionDataByEstimate((int)scheduleDto.estimate_id.Value);
            _TrackingEngine.TrackDetectionByEstimateId(scheduleDto.estimate_id.Value);

            return true;
        }

        public bool DeleteDetectionFile(int bvsFileId)
        {
            _BroadcastDataRepositoryFactory.GetDataRepository<IDetectionRepository>().DeleteById(bvsFileId);
            return true;
        }

        private AabAdvertiserDto _MapToLoadSchedulesDto(AdvertiserDto advertiser)
        {
            return new AabAdvertiserDto
            {
                Id = advertiser.MasterId,
                Display = advertiser.Name
            };
        }
    }
}