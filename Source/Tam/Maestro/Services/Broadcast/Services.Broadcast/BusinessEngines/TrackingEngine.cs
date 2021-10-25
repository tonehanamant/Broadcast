using Common.Services;
using Common.Services.ApplicationServices;
using Common.Services.Extensions;
using Common.Services.Repositories;
using Services.Broadcast.Entities;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.BusinessEngines
{
    public interface ITrackingEngine : IApplicationService
    {
        void TrackDetectionByEstimateId(int estimateId);
        void TrackDetectionByDetectionDetails(List<int> bvsDetailIds, int estimateId);
        void TrackDetectionByDetectionFileId(int bvsId);
        DetectionTrackingDetail AcceptScheduleLeadIn(AcceptScheduleLeadinRequest request);
        DetectionTrackingDetail AcceptScheduleBlock(AcceptScheduleBlockRequest request);
        ProgramMappingDto GetProgramMappingDto(int bvsDetailId);
    }

    public class TrackingEngine : BroadcastBaseClass, ITrackingEngine
    {
        private readonly IDataRepositoryFactory _DataRepositoryFactory;
        private readonly IDaypartCache _DaypartCache;

        private enum DetectionMapTypes
        {
            Station = 1
            , Program = 2
        }

        private Dictionary<string, List<string>> _StationMaps;
        private Dictionary<string, List<string>> _ProgramMaps;

        private Lazy<int> _BroadcastMatchingBuffer;
        public TrackingEngine(IDataRepositoryFactory broadcastDataRepositoryFactory, IDaypartCache daypartCache, IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper)
        : base(featureToggleHelper, configurationSettingsHelper)
        {
            _DataRepositoryFactory = broadcastDataRepositoryFactory;
            _DaypartCache = daypartCache;
            _BroadcastMatchingBuffer = new Lazy<int>(_GetBroadcastMatchingBuffer);
        }

        public DetectionTrackingDetail AcceptScheduleLeadIn(AcceptScheduleLeadinRequest request)
        {
            using (var trx = new TransactionScopeWrapper())
            {
                var scheduleDetailWeek = _DataRepositoryFactory.GetDataRepository<IScheduleRepository>().GetScheduleDetailWeek(request.ScheduleDetailWeekId);
                var detectionDetail = _DataRepositoryFactory.GetDataRepository<IDetectionRepository>().GetDetectionTrackingDetailById(request.DetectionDetailId);

                detectionDetail.LinkedToLeadin = true;
                detectionDetail.ScheduleDetailWeekId = request.ScheduleDetailWeekId;
                detectionDetail.Status = TrackingStatus.InSpec;

                _DataRepositoryFactory.GetDataRepository<IDetectionRepository>().PersistDetectionDetails(
                    new List<DetectionTrackingDetail>
                    {
                        detectionDetail
                    });

                scheduleDetailWeek.FilledSpots++;
                var sceduleSpotTarget = new ScheduleSpotTarget
                {
                    ScheduleDetailWeek = scheduleDetailWeek
                };
                _DataRepositoryFactory.GetDataRepository<IScheduleRepository>().PersistScheduleSpotTargets(
                    new List<ScheduleSpotTarget>
                    {
                        sceduleSpotTarget
                    });

                trx.Complete();

                return detectionDetail;
            }
        }

        public DetectionTrackingDetail AcceptScheduleBlock(AcceptScheduleBlockRequest request)
        {
            using (var trx = new TransactionScopeWrapper())
            {
                var scheduleDetailWeek = _DataRepositoryFactory.GetDataRepository<IScheduleRepository>().GetScheduleDetailWeek(request.ScheduleDetailWeekId);
                var detectionDetail = _DataRepositoryFactory.GetDataRepository<IDetectionRepository>().GetDetectionTrackingDetailById(request.DetectionDetailId);

                detectionDetail.LinkedToBlock = true;
                detectionDetail.ScheduleDetailWeekId = request.ScheduleDetailWeekId;
                detectionDetail.Status = TrackingStatus.InSpec;

                _DataRepositoryFactory.GetDataRepository<IDetectionRepository>().PersistDetectionDetails(
                    new List<DetectionTrackingDetail>
                    {
                        detectionDetail
                    });

                scheduleDetailWeek.FilledSpots++;
                var sceduleSpotTarget = new ScheduleSpotTarget
                {
                    ScheduleDetailWeek = scheduleDetailWeek
                };
                _DataRepositoryFactory.GetDataRepository<IScheduleRepository>().PersistScheduleSpotTargets(
                    new List<ScheduleSpotTarget>
                    {
                        sceduleSpotTarget
                    });

                trx.Complete();

                return detectionDetail;
            }
        }

        public void TrackDetectionByDetectionFileId(int bvsId)
        {
            //get the unique estimates with matching schedules from the BVS details
            var estimateIds = _DataRepositoryFactory.GetDataRepository<IScheduleRepository>().GetScheduleEstimateIdsByDetectionFile(bvsId);
            foreach (var estimateId in estimateIds)
            {
                TrackDetectionByEstimateId(estimateId);
            }
        }

        public void TrackDetectionByEstimateId(int estimateId)
        {
            var scheduleItems = _DataRepositoryFactory.GetDataRepository<IScheduleRepository>().GetScheduleTrackingDetails(estimateId);
            var bvsItems = _DataRepositoryFactory.GetDataRepository<IDetectionRepository>().GetDetectionTrackingDetailsByEstimateId(estimateId);
            _TrackBvsData(bvsItems, scheduleItems, estimateId);
        }

        public void TrackDetectionByDetectionDetails(List<int> bvsDetailIds, int estimateId)
        {
            var scheduleItems = _DataRepositoryFactory.GetDataRepository<IScheduleRepository>().GetScheduleTrackingDetails(estimateId);
            var bvsItems = _DataRepositoryFactory.GetDataRepository<IDetectionRepository>().GetDetectionTrackingDetailsByDetailIds(bvsDetailIds);
            _TrackBvsData(bvsItems, scheduleItems, estimateId);
        }

        private void _TrackBvsData(List<DetectionTrackingDetail> bvsDetails, List<ScheduleDetail> scheduleItems, int estimateId)
        {
            var scheduleSpotTargets = scheduleItems.SelectMany(x => x.ToScheduleSpotTargets()).ToList();
            var bvsDetailsToProcess = bvsDetails.Where(x => !x.LinkedToBlock && !x.LinkedToLeadin).ToList();

            //Remove the FilledSpots for BVS data that will be re-processed.
            foreach (var bvsDetail in bvsDetailsToProcess.Where(x => x.ScheduleDetailWeekId != null))
            {
                var scheduleSpotTarget = scheduleSpotTargets.Single(x => x.ScheduleDetailWeek.ScheduleDetailWeekId == bvsDetail.ScheduleDetailWeekId);
                scheduleSpotTarget.ScheduleDetailWeek.FilledSpots--;
            }

            _ClearTrackingInfo(bvsDetailsToProcess);

            var schedule = _DataRepositoryFactory.GetDataRepository<IScheduleRepository>().GetScheduleDtoByEstimateId(estimateId);
            var bvsStations = bvsDetailsToProcess.Select(x => x.Station).Distinct().ToList();

            foreach (var bvsDetail in bvsDetails)
            {
                bvsDetail.MatchIsci = schedule.ContainsIsci(bvsDetail.Isci);
            }
            
            // Iterate through stations to get every daypart needed for this file, then add them all to the cache in one go
            // Improves performance by reducing database connections
            var daypartsToCache = new List<int>();
            bvsStations.ForEach(station =>
            {
                var bvsDetailsByStation = bvsDetailsToProcess.Where(x => x.Station == station).ToList();
                bvsDetailsByStation.ForEach(detail =>
                {
                    var stationSchedules = _FindDeliveryStationMatch(station, scheduleSpotTargets);
                    var daypartIds = stationSchedules.Select(d => d.ScheduleDetail.DaypartId).ToList();
                    daypartsToCache.AddRange(daypartIds);
                });
            });
            _DaypartCache.GetDisplayDayparts(daypartsToCache);

            foreach (var station in bvsStations)
            {
                var bvsDetailsByStation = bvsDetailsToProcess.Where(x => x.Station == station).OrderBy(d => d.NsiDate).ThenBy(t => t.AirTime).ToList();
                foreach (var bvsDetail in bvsDetailsByStation)
                {
                    var activeMatches = scheduleSpotTargets;
                    var stationSchedules = _FindDeliveryStationMatch(station, scheduleSpotTargets);
                    if (stationSchedules.Any())
                    {
                        activeMatches = stationSchedules;
                        bvsDetail.MatchStation = true;
                    }

                    var newAirtimeMatches = _FindDeliveryTimeMatch(bvsDetail, activeMatches);
                    if (newAirtimeMatches.Any())
                    {
                        activeMatches = newAirtimeMatches;

                        bvsDetail.MatchAirtime = true;
                        bvsDetail.HasLeadInScheduleMatches = newAirtimeMatches.Any(x => x.IsLeadInMatch);
                    }
                    else
                    {
                        bvsDetail.Status = TrackingStatus.OfficialOutOfSpec;
                    }

                    var newProgramMatches = _FindDeliveryProgramMatch(bvsDetail.Program, activeMatches);
                    if (newProgramMatches.Any())
                    {
                        bvsDetail.MatchProgram = true;
                        activeMatches = newProgramMatches;
                    }
                    var newSpotLengthMatches = activeMatches.Where(a => a.ScheduleDetail.SpotLengthId == bvsDetail.SpotLengthId).ToList();
                    if (newSpotLengthMatches.Any())
                    {
                        bvsDetail.MatchSpotLength = true;
                        activeMatches = newSpotLengthMatches;
                    }

                    var programAndLeadInMatch = (activeMatches.Any(x => x.IsLeadInMatch == false) && activeMatches.Any(x => x.IsLeadInMatch == true));

                    if (activeMatches.Count == 1 && bvsDetail.MatchAirtime && bvsDetail.MatchStation && bvsDetail.MatchIsci 
                        && bvsDetail.MatchProgram && bvsDetail.MatchSpotLength)
                    {
                        bvsDetail.ScheduleDetailWeekId = activeMatches.Single().ScheduleDetailWeek.ScheduleDetailWeekId;
                        activeMatches.Single().ScheduleDetailWeek.FilledSpots++;
                    }
                    else if (activeMatches.Count == 2 && bvsDetail.MatchAirtime && bvsDetail.MatchStation && bvsDetail.MatchIsci 
                                && bvsDetail.MatchProgram && bvsDetail.MatchSpotLength && programAndLeadInMatch)
                    {
                        var primaryMatch = activeMatches.Single(x => x.IsLeadInMatch == false, "No Primary Program match Found.");
                        var leadInMatch = activeMatches.Single(x => x.IsLeadInMatch == true, "No Lead In Program match Found.");

                        if (leadInMatch.AllSpotsFilled())
                        {
                            bvsDetail.ScheduleDetailWeekId = primaryMatch.ScheduleDetailWeek.ScheduleDetailWeekId;
                            primaryMatch.ScheduleDetailWeek.FilledSpots++;
                        }
                        else if (primaryMatch.AllSpotsFilled())
                        {
                            bvsDetail.ScheduleDetailWeekId = leadInMatch.ScheduleDetailWeek.ScheduleDetailWeekId;
                            leadInMatch.ScheduleDetailWeek.FilledSpots++;
                        }
                        else
                        {
                            bvsDetail.ScheduleDetailWeekId = primaryMatch.ScheduleDetailWeek.ScheduleDetailWeekId;
                            primaryMatch.ScheduleDetailWeek.FilledSpots++;
                        }
                    }
                    else if (activeMatches.Count >= 2 && bvsDetail.MatchAirtime && bvsDetail.MatchStation && bvsDetail.MatchIsci
                        && bvsDetail.MatchProgram && bvsDetail.MatchSpotLength)
                    {
                        var bestMatch = activeMatches.OrderByDescending(a => a.ScheduleDetail.SpotCost)
                                                        .First();
                        bvsDetail.ScheduleDetailWeekId = bestMatch.ScheduleDetailWeek.ScheduleDetailWeekId;
                        bestMatch.ScheduleDetailWeek.FilledSpots++;
                    }
                }
            }

            foreach (var bvsDetail in bvsDetails)
            {
                if (bvsDetail.Status == TrackingStatus.OfficialOutOfSpec)
                {

                }
                else if (_BvsDetailIsInSpec(bvsDetail))
                {
                    bvsDetail.Status = TrackingStatus.InSpec;
                }
                else    
                {
                    bvsDetail.Status = TrackingStatus.OutOfSpec;
                }
            }

            _DataRepositoryFactory.GetDataRepository<IScheduleRepository>().PersistScheduleSpotTargets(scheduleSpotTargets);
            _DataRepositoryFactory.GetDataRepository<IDetectionRepository>().PersistDetectionDetails(bvsDetails);
        }

        private bool _BvsDetailIsInSpec(DetectionTrackingDetail bvsDetail)
        {
            return (bvsDetail.MatchAirtime && bvsDetail.MatchStation && bvsDetail.MatchIsci && bvsDetail.ScheduleDetailWeekId != null && bvsDetail.MatchSpotLength);
        }

        private void _ClearTrackingInfo(List<DetectionTrackingDetail> bvsDetails)
        {
            foreach (var bvsDetail in bvsDetails)
            {
                // prevents clearing the status of OfficialOutOfSpec entries
                bvsDetail.Status = bvsDetail.Status != TrackingStatus.OfficialOutOfSpec ? TrackingStatus.UnTracked : bvsDetail.Status;

                bvsDetail.MatchAirtime = false;
                bvsDetail.MatchProgram = false;
                bvsDetail.MatchStation = false;
                bvsDetail.MatchIsci = false;
                bvsDetail.MatchSpotLength = false;
                bvsDetail.ScheduleDetailWeekId = null;
            }
        }

        private List<ScheduleSpotTarget> _FindDeliveryStationMatch(string bvsStation, List<ScheduleSpotTarget> scheduleSpotTargets)
        {
            var ret = new List<ScheduleSpotTarget>();

            foreach (var scheduleSpotTarget in scheduleSpotTargets)
            {
                var mappedValues = _GetMappedStation(bvsStation);
                if (bvsStation == scheduleSpotTarget.ScheduleDetail.Station || mappedValues.Contains(scheduleSpotTarget.ScheduleDetail.Station))
                    ret.Add(scheduleSpotTarget);
            }
            return ret;
        }

        private List<ScheduleSpotTarget> _FindDeliveryProgramMatch(string bvsProgram,
            List<ScheduleSpotTarget> scheduleSpotTargets)
        {
            var ret = new List<ScheduleSpotTarget>();
            foreach (var scheduleSpotTarget in scheduleSpotTargets)
            {
                var mappedValues = _GetMappedProgram(bvsProgram).Select(x => x.Trim()).ToList();
                if (
                    string.Equals(bvsProgram.Trim(), scheduleSpotTarget.ScheduleDetail.Program.Trim(),
                        StringComparison.OrdinalIgnoreCase) ||
                    mappedValues.Contains(scheduleSpotTarget.ScheduleDetail.Program.Trim(), StringComparer.OrdinalIgnoreCase))
                {
                    ret.Add(scheduleSpotTarget);
                }
            }
            return ret;
        }

        private List<ScheduleSpotTarget> _FindDeliveryTimeMatch(
                                                    DetectionTrackingDetail bvsTrackingDetail, 
                                                    List<ScheduleSpotTarget> scheduleDetails)
        {
            var ret = new List<ScheduleSpotTarget>();

            var timeMatchedDetails = new List<ScheduleSpotTarget>();
            var dayparts = _DaypartCache.GetDisplayDayparts(scheduleDetails.Select(d => d.ScheduleDetail.DaypartId));
            foreach (var scheduleDetail in scheduleDetails)
            {
                var bufferInSeconds = _BroadcastMatchingBuffer.Value;

                var displayDaypart = dayparts[scheduleDetail.ScheduleDetail.DaypartId];
                var actualStartTime = displayDaypart.StartTime < 0 ? BroadcastConstants.OneDayInSeconds - Math.Abs(displayDaypart.StartTime) : displayDaypart.StartTime;
                var actualEndTime = displayDaypart.EndTime < 0 ? Math.Abs(BroadcastConstants.OneDayInSeconds - displayDaypart.EndTime) : displayDaypart.EndTime;
                var adjustedStartTime = displayDaypart.StartTime - bufferInSeconds < 0 ? BroadcastConstants.OneDayInSeconds - Math.Abs(displayDaypart.StartTime - bufferInSeconds) : displayDaypart.StartTime - bufferInSeconds;

                var isOvernight = (actualEndTime < actualStartTime && actualEndTime < adjustedStartTime);
                if (isOvernight)
                {
                    // some of these "if" can be combined, but will be harder to maintain and negligably performant
                    if (bvsTrackingDetail.AirTime >= actualStartTime && bvsTrackingDetail.AirTime >= actualEndTime)
                    {   // covers airtime before midnight
                        scheduleDetail.IsLeadInMatch = false;
                        timeMatchedDetails.Add(scheduleDetail);
                    }
                    else if (bvsTrackingDetail.AirTime >= adjustedStartTime && bvsTrackingDetail.AirTime >= actualEndTime)
                    {   // covers lead in time
                        scheduleDetail.IsLeadInMatch = true;
                        timeMatchedDetails.Add(scheduleDetail);
                    }
                    else if (bvsTrackingDetail.AirTime <= actualEndTime && bvsTrackingDetail.AirTime <= actualStartTime)
                    {   // covers airtime after midnight
                        scheduleDetail.IsLeadInMatch = false;
                        timeMatchedDetails.Add(scheduleDetail);
                    }
                }
                else
                {
                    if (bvsTrackingDetail.AirTime >= actualStartTime && bvsTrackingDetail.AirTime <= actualEndTime)
                    {
                        scheduleDetail.IsLeadInMatch = false;
                        timeMatchedDetails.Add(scheduleDetail);
                    }
                    else if (bvsTrackingDetail.AirTime >= adjustedStartTime && bvsTrackingDetail.AirTime <= actualEndTime)
                    {
                        scheduleDetail.IsLeadInMatch = true;
                        timeMatchedDetails.Add(scheduleDetail);
                    }
                }
            }

            foreach (var timeMatchedDetail in timeMatchedDetails)
            {
                var displayDaypart = _DaypartCache.GetDisplayDaypart(timeMatchedDetail.ScheduleDetail.DaypartId);

                if (bvsTrackingDetail.NsiDate >= timeMatchedDetail.ScheduleDetailWeek.StartDate &&
                    bvsTrackingDetail.NsiDate <= timeMatchedDetail.ScheduleDetailWeek.EndDate &&
                    timeMatchedDetail.ScheduleDetailWeek.Spots != 0)
                {
                    if (displayDaypart.Days.Contains(bvsTrackingDetail.NsiDate.DayOfWeek))
                    {
                        timeMatchedDetail.ScheduleDetailWeek.Matched = true;
                        ret.Add(timeMatchedDetail);
                    }
                }
            }

            return ret;
        }

        private List<string> _GetMappedStation(string bvsStation)
        {
            if (_StationMaps == null)
            {
                _StationMaps = _GetMapDictionary((int)DetectionMapTypes.Station);
            }

            List<string> mappedStation;
            return _StationMaps.TryGetValue(bvsStation, out mappedStation) ? mappedStation : new List<string>();
        }

        private List<string> _GetMappedProgram(string affidavitProgram)
        {
            if (_ProgramMaps == null)
            {
                _ProgramMaps = _GetMapDictionary((int)DetectionMapTypes.Program);
            }
            return _ProgramMaps.ContainsKey(affidavitProgram) ? _ProgramMaps[affidavitProgram].ToList() : new List<string>();
        }

        private Dictionary<string, List<string>> _GetMapDictionary(int mapType)
        {
            var ret = new Dictionary<string, List<string>>();
            var map = _DataRepositoryFactory.GetDataRepository<ITrackerMappingRepository>().GetMap(mapType);

            foreach (var mapValue in map.TrackingMapValues)
            {
                if (!ret.ContainsKey(mapValue.DetectionValue)) ret[mapValue.DetectionValue] = new List<string>();
                ret[mapValue.DetectionValue].Add(mapValue.ScheduleValue);
            }
            return ret;
        }

        public ProgramMappingDto GetProgramMappingDto(int bvsDetailId)
        {
            var bvsDetail = _DataRepositoryFactory.GetDataRepository<IDetectionRepository>().GetDetectionTrackingDetailById(bvsDetailId);

            if (string.IsNullOrWhiteSpace(bvsDetail.Program))
                return new ProgramMappingDto();

            var scheduleDetails = _DataRepositoryFactory.GetDataRepository<IScheduleRepository>().GetScheduleTrackingDetails(bvsDetail.EstimateId);

            var scheduleSpotTargets = scheduleDetails.SelectMany(x => x.ToScheduleSpotTargets()).ToList();

            var stationSchedules = _FindDeliveryStationMatch(bvsDetail.Station, scheduleSpotTargets);
            var airDateSchedules = stationSchedules.Where(x => x.ScheduleDetailWeek.StartDate <= bvsDetail.NsiDate && x.ScheduleDetailWeek.EndDate >= bvsDetail.NsiDate).ToList();
            var potentialMatches = _FindDeliveryTimeMatch(bvsDetail, airDateSchedules);

            return new ProgramMappingDto
            {
                DetectionProgramName = bvsDetail.Program,
                PrimaryScheduleMatches = potentialMatches.Where(x => !x.IsLeadInMatch).Select(
                    x => new ScheduleProgramMappingDto
                    {
                        ScheduleDetailWeekId = x.ScheduleDetailWeek.ScheduleDetailWeekId,
                        ProgramName = x.ScheduleDetail.Program,
                        ScheduleDaypart = _DaypartCache.GetDisplayDaypart(x.ScheduleDetail.DaypartId).ToString(),
                    }).ToList(),
                FollowingScheduleMatches = potentialMatches.Where(x => x.IsLeadInMatch).Select(
                    x => new ScheduleProgramMappingDto
                    {
                        ScheduleDetailWeekId = x.ScheduleDetailWeek.ScheduleDetailWeekId,
                        ProgramName = x.ScheduleDetail.Program,
                        ScheduleDaypart = _DaypartCache.GetDisplayDaypart(x.ScheduleDetail.DaypartId).ToString(),
                    }).ToList(),
            };
        }
        private int _GetBroadcastMatchingBuffer()
        {
            var broadcastMatchingBuffer = _ConfigurationSettingsHelper.GetConfigValueWithDefault(ConfigKeys.BroadcastMatchingBuffer, 120);
            return broadcastMatchingBuffer;
        }
    }
}
