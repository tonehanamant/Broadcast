using Common.Services;
using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Converters;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Tam.Maestro.Common;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.Cable.SystemComponentParameters;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.ApplicationServices
{

    public interface ITrackerService : IApplicationService
    {
        LoadSchedulesDto GetSchedulesByDate(DateTime? startDate, DateTime? endDate);
        BvsLoadDto GetBvsLoadData(DateTime currentDateTime);
        List<LookupDto> GetNsiPostingBookMonths();
        int SaveSchedule(ScheduleSaveRequest request);
        Tuple<List<int>, string> SaveBvsFiles(BvsSaveRequest request);
        string SaveBvsViaFtp(string userName);
        bool ScheduleExists(int estimateIds);
        BvsScrubbingDto GetBvsScrubbingData(int estimateId);
        List<LookupDto> GetSchedulePrograms(int scheduleId);
        List<LookupDto> GetScheduleStations(int scheduleId);
        List<BvsTrackingDetail> SaveScrubbingMapping(ScrubbingMap map);
        List<ScheduleDetail> GetScheduleDetailsByEstimateId(int estimateId);
        Dictionary<int, int> GetScheduleAudiences(int estimateId);
        bool ScrubSchedule(ScheduleScrubbing scheduleScrubbing);
        BvsMap GetBvsMapByType(string mappingType);
        bool DeleteMapping(string mappingType, TrackingMapValue mapping);
        ScheduleHeaderDto GetScheduleHeader(int estimateId);
        DisplaySchedule GetDisplayScheduleById(int scheduleId);
        bool UpdateRatingAdjustments(List<RatingAdjustmentsDto> ratingAdjustments);
        RatingAdjustmentsResponse GetRatingAdjustments();
        List<BvsFileSummary> GetBvsFileSummaries();
        bool TrackSchedule(int scheduleId);

        bool DeleteBvsFile(int bvsFileId);
    }

    public class TrackerService : ITrackerService
    {
        private const int MaxFTPFailuresAllow = 1; // 1 for now, maybe more in the future

        private readonly IDataRepositoryFactory _BroadcastDataRepositoryFactory;
        private readonly IBvsPostingEngine _BvsPostingEngine;
        private readonly ITrackingEngine _TrackingEngine;
        private readonly IScxConverter _ScxConverter;
        private readonly IBvsConverter _BvsConverter;
        private readonly IAssemblyScheduleConverter _AssemblyFileConverter;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekAggregateCache;
        private readonly IBroadcastAudiencesCache _AudiencesCache;
        private readonly IDefaultScheduleConverter _DefaultScheduleConverter;
        private readonly IDaypartCache _DayPartCache;
        private readonly IQuarterCalculationEngine _QuarterCalculationEngine;
        private readonly ISMSClient _SmsClient;
        private readonly IImpressionAdjustmentEngine _ImpressionAdjustmentEngine;

        public TrackerService(IDataRepositoryFactory broadcastDataRepositoryFactory, IBvsPostingEngine bvsPostingEngine,
            ITrackingEngine trackingEngine, IScxConverter scxConverter, IBvsConverter bvsConverter,
            IAssemblyScheduleConverter assemblyFileConverter,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache, IBroadcastAudiencesCache audiencesCache,
            IDefaultScheduleConverter defaultScheduleConverter, IDaypartCache dayPartCache,
            IQuarterCalculationEngine quarterCalculationEngine, ISMSClient smsClient, IImpressionAdjustmentEngine impressionAdjustmentEngine)
        {
            _BroadcastDataRepositoryFactory = broadcastDataRepositoryFactory;
            _BvsPostingEngine = bvsPostingEngine;
            _TrackingEngine = trackingEngine;
            _ScxConverter = scxConverter;
            _BvsConverter = bvsConverter;
            _AssemblyFileConverter = assemblyFileConverter;
            _MediaMonthAndWeekAggregateCache = mediaMonthAndWeekAggregateCache;
            _AudiencesCache = audiencesCache;
            _DefaultScheduleConverter = defaultScheduleConverter;
            _DayPartCache = dayPartCache;
            _QuarterCalculationEngine = quarterCalculationEngine;
            _SmsClient = smsClient;
            _ImpressionAdjustmentEngine = impressionAdjustmentEngine;
        }

        public LoadSchedulesDto GetSchedulesByDate(DateTime? startDate, DateTime? endDate)
        {
            var ret = new LoadSchedulesDto();

            ret.Schedules = GetDisplaySchedulesWithAdjustedImpressions(startDate, endDate);

            var scheduleAdvertisers = ret.Schedules.Select(x => x.AdvertiserId).ToList();
            ret.Advertisers = _SmsClient.GetActiveAdvertisers().Where(a => scheduleAdvertisers.Contains(a.Id)).ToList();
            ret.PostingBooks = GetNsiPostingBookMonths();
            foreach (var schedule in ret.Schedules)
            {
                var advertiser = ret.Advertisers.FirstOrDefault(a => a.Id == schedule.AdvertiserId);
                var postingBook = ret.PostingBooks.FirstOrDefault(p => p.Id == schedule.PostingBookId);
                schedule.Advertiser = advertiser == null ? "" : advertiser.Display;
                schedule.PostingBook = postingBook == null ? "" : postingBook.Display;

            }

            return ret;
        }

        internal List<DisplaySchedule> GetDisplaySchedulesWithAdjustedImpressions(DateTime? startDate, DateTime? endDate)
        {
            var displaySchedules = _BroadcastDataRepositoryFactory.GetDataRepository<IScheduleRepository>().GetDisplaySchedules(startDate, endDate);
            foreach (var schedule in displaySchedules)
            {
                if (schedule.PrimaryDemoBooked.HasValue)
                    schedule.PrimaryDemoBooked = _ImpressionAdjustmentEngine.AdjustImpression(schedule.PrimaryDemoBooked.Value, schedule.PostType, schedule.PostingBookId, false);

                schedule.PrimaryDemoDelivered = _ImpressionAdjustmentEngine.AdjustImpression(schedule.PrimaryDemoDelivered, schedule.PostType, schedule.PostingBookId, false);
            }
            return displaySchedules;
        }

        public BvsLoadDto GetBvsLoadData(DateTime currentDateTime)
        {
            var ret = new BvsLoadDto();
            ret.Advertisers = _SmsClient.GetActiveAdvertisers();
            ret.Quarters = GetQuarters();
            ret.CurrentQuarter =
                ret.Quarters.Find(
                    x =>
                        x.StartDate.Month <= currentDateTime.Month && x.EndDate.Month >= currentDateTime.Month &&
                        x.StartDate.Year == currentDateTime.Year);
            ret.PostingBooks = GetNsiPostingBookMonths();

            ret.InventorySources =
                Enum.GetValues(typeof(RatesFile.RateSourceType))
                    .Cast<RatesFile.RateSourceType>()
                    .Where(e => e != RatesFile.RateSourceType.Blank)
                    .Select(e => new LookupDto { Display = e.ToString(), Id = (int)e })
                    .ToList();

            ret.SchedulePostTypes =
                Enum.GetValues(typeof(SchedulePostType))
                    .Cast<SchedulePostType>()
                    .Select(e => new LookupDto { Display = e.ToString(), Id = (int)e })
                    .ToList();

            ret.Markets =
                _BroadcastDataRepositoryFactory.GetDataRepository<IMarketRepository>()
                    .GetMarkets()
                    .Select(m => new LookupDto { Display = m.geography_name, Id = m.market_code })
                    .ToList();

            ret.Audiences = _AudiencesCache.GetAllLookups();

            return ret;
        }

        /// <summary>
        /// </summary>
        public List<Quarter> GetQuarters()
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

        public List<LookupDto> GetNsiPostingBookMonths()
        {
            var postingBooks =
                _BroadcastDataRepositoryFactory.GetDataRepository<IPostingBookRepository>()
                    .GetPostableMediaMonths(BroadcastConstants.PostableMonthMarketThreshold);

            var mediaMonths = _MediaMonthAndWeekAggregateCache.GetMediaMonthsByIds(postingBooks);

            return (from x in mediaMonths
                    select new LookupDto
                    {
                        Id = x.Id
                    ,
                        Display = x.MediaMonthX
                    }).ToList();
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
                _BvsPostingEngine.PostBvsDataByEstimate((int)scheduleDto.EstimateId);
                _TrackingEngine.TrackBvsByEstimateId((int)scheduleDto.EstimateId);
            }
            else if (converter is IDefaultScheduleConverter)
            {
                var iscis = efSchedule.schedule_iscis.Select(si => si.house_isci).ToList();
                var scheduleAudiences = efSchedule.schedule_audiences.ToDictionary(sa => sa.rank ?? 0, sa => sa.audience_id);
                _BvsPostingEngine.PostBvsData(iscis, scheduleAudiences, scheduleDto.PostingBookId);
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
                var bvsRepo = _BroadcastDataRepositoryFactory.GetDataRepository<IBvsRepository>();
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
                var bvsRepo = _BroadcastDataRepositoryFactory.GetDataRepository<IBvsRepository>();
                var officiallyOutOfSpecBvsItems = bvsRepo.GetBvsTrackingDetailsByDetailIds(scheduleScrubbing.OfficiallyOutOfSpecIds);
                officiallyOutOfSpecBvsItems.ForEach(c => c.Status = TrackingStatus.OfficialOutOfSpec);
                bvsRepo.PersistBvsDetails(officiallyOutOfSpecBvsItems);
            }

            var scheduleRepo = _BroadcastDataRepositoryFactory.GetDataRepository<IScheduleRepository>();
            var efSchedule = scheduleRepo.FindByEstimateId(scheduleScrubbing.EstimateId);

            var postingBookUpdated = scheduleScrubbing.PostingBookId != efSchedule.posting_book_id;
            if (efSchedule.estimate_id != null && postingBookUpdated)
            {
                scheduleRepo.UpdateSchedulePostingBook(efSchedule.id, scheduleScrubbing.PostingBookId);

                // Reevaluates the RatingsAggregates (delivery) for each station
                _BvsPostingEngine.PostBvsDataByEstimate((int)efSchedule.estimate_id);
            }

            return true;
        }

        /// <summary>
        /// Returns bvsID.
        /// </summary>
        public Tuple<List<int>, string> SaveBvsFiles(BvsSaveRequest request)
        {
            var duplicateMessage = string.Empty;

            var bvsIds = new List<int>();
            var bvsRepo = _BroadcastDataRepositoryFactory.GetDataRepository<IBvsRepository>();
            var errorMessage = string.Empty;
            var hasErrors = false;

            foreach (var requestBvsFile in request.BvsFiles)
            {
                try
                {
                    errorMessage += string.Format("Starting upload for file '{0}' . . .", requestBvsFile.BvsFileName);

                    //compute file hash to check against duplicate files being loaded
                    var hash = HashGenerator.ComputeHash(StreamHelper.ReadToEnd(requestBvsFile.BvsStream));

                    //check if file has already been loaded
                    if (bvsRepo.GetBvsFileIdByHash(hash) > 0)
                    {
                        throw new ApplicationException("Unable to load BVS file. The BVS file selected has already been loaded.");
                    }

                    string message;
                    //we made it this far, it must be a new file - persist the file
                    Dictionary<BvsFileDetailKey, int> lineInfo;
                    var bvsFile = _BvsConverter.ExtractBvsData(requestBvsFile.BvsStream, hash, request.UserName, requestBvsFile.BvsFileName, out message, out lineInfo);

                    if (!string.IsNullOrEmpty(message))
                    {
                        errorMessage += "<br />" + message;
                        hasErrors = true;
                    }

                    var filterResult = bvsRepo.FilterOutExistingDetails(bvsFile.bvs_file_details.ToList());
                    bvsFile.bvs_file_details = filterResult.New;

                    if (filterResult.Ignored.Any())
                    {
                        duplicateMessage += "<p>The following line(s) were previously imported and were ignored:</p><ul>";
                        foreach (var file in filterResult.Ignored)
                        {
                            duplicateMessage += string.Format("<li>Line {0}: Station {1}, Date {2}, Time Aired {3}, ISCI {4}, Spot Length {5}, Campaign {6}, Advertiser {7}</li>",
                                lineInfo[new BvsFileDetailKey(file)], file.station, file.date_aired, file.time_aired, file.isci, file.spot_length, file.estimate_id, file.advertiser);
                        }

                        duplicateMessage += "</ul>";
                    }

                    if (filterResult.Updated.Any())
                    {
                        duplicateMessage += "<p>The following line(s) were previously imported and were updated with new program name:</p><ul>";
                        foreach (var file in filterResult.Updated)
                        {
                            duplicateMessage += string.Format("<li>Line {0}: Station {1}, Date {2}, Time Aired {3}, ISCI {4}, Spot Length {5}, Campaign {6}, Advertiser {7}, Program Name {8}</li>",
                                lineInfo[new BvsFileDetailKey(file)], file.station, file.date_aired, file.time_aired, file.isci, file.spot_length, file.estimate_id, file.advertiser, file.program_name);
                        }

                        duplicateMessage += "</ul>";
                    }

                    bvsIds.Add(bvsRepo.SaveBvsFile(bvsFile));
                    errorMessage += string.Format("File '{0}' uploaded successfully.<br />", requestBvsFile.BvsFileName);
                }
                catch (Exception e)
                {
                    hasErrors = true;
                    errorMessage += string.Format("<br /> Error processing file '{0}'. <br />Message:<br />{1}<br />", requestBvsFile.BvsFileName, e.Message);
                }
            }


            var estimateIds = _BroadcastDataRepositoryFactory.GetDataRepository<IBvsRepository>().GetEstimateIdsWithSchedulesByFileIds(bvsIds);
            foreach (var estimateId in estimateIds)
            {
                //post for delivery
                _BvsPostingEngine.PostBvsDataByEstimate(estimateId);

                _TrackingEngine.TrackBvsByEstimateId(estimateId);
            }

            if (hasErrors)
                throw new Exception(errorMessage);

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

        public string SaveBvsViaFtp(string userName)
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
                            var request = new BvsSaveRequest();
                            request.UserName = userName;
                            request.BvsFiles.Add(new BvsFile { BvsFileName = fileName, BvsStream = stream });

                            //Requirements are to ignore this for FTP for now
                            SaveBvsFiles(request);

                            successFiles += fileName + Environment.NewLine;
                        }
                        catch (ExtractBvsExceptionEmptyFiles)
                        {
                            failMessages += string.Format("<br />Empty File: <i>{0}</i><br />", fileName);
                            continue;
                        }
                        catch (ExtractBvsExceptionCableTv)
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

        private static NetworkCredential _GetCredentials()
        {
            var pwd = EncryptionHelper.DecryptString(TrackerServiceSystemParameter.FtpPassword, EncryptionHelper.EncryptionKey);
            return new NetworkCredential(TrackerServiceSystemParameter.FtpUserName, pwd);
        }

        private static IEnumerable<string> _GetFtpFilesAndNames()
        {
            var fileList = new List<string>();

            // Get the object used to communicate with the server.
            var ftp = (FtpWebRequest)WebRequest.Create(TrackerServiceSystemParameter.FtpUrl + "/" +
                                  TrackerServiceSystemParameter.FtpDirectory);

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

        private static MemoryStream _GetFileStream(string fileName)
        {
            var path = TrackerServiceSystemParameter.FtpUrl + "/" + TrackerServiceSystemParameter.FtpDirectory + "/" + fileName;
            using (var ftpClient = new WebClient())
            {
                ftpClient.Credentials = _GetCredentials();
                var stream = new MemoryStream(ftpClient.DownloadData(path));
                return stream;
            }
        }

        public void _CleanupFile(MemoryStream memoryStream, string fileName)
        {
            var path = TrackerServiceSystemParameter.FtpSaveFolder + "\\" + fileName;
            var fileStream = File.OpenWrite(path);
            memoryStream.WriteTo(fileStream);

            var fileUrl = TrackerServiceSystemParameter.FtpUrl + "/" + TrackerServiceSystemParameter.FtpDirectory + "/" + fileName;
            _DeleteFtpFile(fileUrl);
        }

        private static void _DeleteFtpFile(string fileUrl)
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

        public BvsScrubbingDto GetBvsScrubbingData(int estimateId)
        {
            var scrubbingDto = new BvsScrubbingDto();

            var schedule = _BroadcastDataRepositoryFactory.GetDataRepository<IScheduleRepository>().GetScheduleDtoByEstimateId(estimateId);

            scrubbingDto.CurrentPostingBookId = schedule.PostingBookId;
            scrubbingDto.EstimateId = schedule.EstimateId;
            scrubbingDto.ScheduleName = schedule.ScheduleName;
            scrubbingDto.ISCIs = string.Join(",", schedule.ISCIs.Select(i => i.House));
            scrubbingDto.PostingBooks = GetNsiPostingBookMonths();
            scrubbingDto.BvsDetails = GetBvsDetailsWithAdjustedImpressions(estimateId, schedule);
            scrubbingDto.SchedulePrograms = _BroadcastDataRepositoryFactory.GetDataRepository<IScheduleRepository>().GetScheduleLookupPrograms(schedule.Id);
            scrubbingDto.ScheduleNetworks = _BroadcastDataRepositoryFactory.GetDataRepository<IScheduleRepository>().GetScheduleLookupStations(schedule.Id);

            return scrubbingDto;
        }

        internal List<BvsTrackingDetail> GetBvsDetailsWithAdjustedImpressions(int estimateId, ScheduleDTO schedule)
        {
            var details = _BroadcastDataRepositoryFactory.GetDataRepository<IBvsRepository>().GetBvsTrackingDetailsByEstimateId(estimateId);
            foreach (var detail in details)
            {
                if (detail.Impressions.HasValue)
                    detail.Impressions = _ImpressionAdjustmentEngine.AdjustImpression(detail.Impressions.Value, schedule.PostType, schedule.PostingBookId, false);
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

        public List<BvsTrackingDetail> SaveScrubbingMapping(ScrubbingMap map)
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
            _TrackingEngine.TrackBvsByBvsDetails(map.DetailIds, map.EstimateId);
            return _BroadcastDataRepositoryFactory.GetDataRepository<IBvsRepository>().GetBvsTrackingDetailsByDetailIds(map.DetailIds);
        }

        public Dictionary<int, int> GetScheduleAudiences(int estimateId)
        {
            return _BroadcastDataRepositoryFactory.GetDataRepository<IScheduleRepository>().GetDictionaryOfScheduleAudiencesByRank(estimateId);
        }

        public List<ScheduleDetail> GetScheduleDetailsByEstimateId(int estimateId)
        {
            return _BroadcastDataRepositoryFactory.GetDataRepository<IScheduleRepository>().GetScheduleTrackingDetails(estimateId);
        }

        public BvsMap GetBvsMapByType(string mappingTypeString)
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
                .DeleteMapping(mapping.BvsValue, mapping.ScheduleValue, mappingType);
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
            var postingBooks = GetNsiPostingBookMonths();

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

        public List<BvsFileSummary> GetBvsFileSummaries()
        {
            return _BroadcastDataRepositoryFactory.GetDataRepository<IBvsRepository>().GetBvsFileSummaries();
        }

        public bool TrackSchedule(int scheduleId)
        {
            var scheduleRepo = _BroadcastDataRepositoryFactory.GetDataRepository<IScheduleRepository>();
            var scheduleDto = scheduleRepo.GetById(scheduleId);
            if (scheduleDto == null || scheduleDto.estimate_id == null)
                throw new Exception("Could not load schedule from Id=" + scheduleId);

            _TrackingEngine.TrackBvsByEstimateId(scheduleDto.estimate_id.Value);

            return true;
        }

        public bool DeleteBvsFile(int bvsFileId)
        {
            _BroadcastDataRepositoryFactory.GetDataRepository<IBvsRepository>().DeleteById(bvsFileId);
            return true;
        }
    }
}

