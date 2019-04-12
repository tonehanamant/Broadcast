using System.Collections.Generic;
using System.Linq;
using Common.Services;
using Common.Services.Repositories;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.DTO;
using Services.Broadcast.Entities.OpenMarketInventory;
using Services.Broadcast.Entities.Scx;
using Services.Broadcast.Repositories;
using Tam.Maestro.Services.Cable.SystemComponentParameters;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.Converters.Scx
{
    public interface IProposalScxDataPrep
    {
        ScxData GetDataFromProposalDetail(ProposalDto dto, ProposalDetailDto proposalDetailDto);
    }

    public class ProposalScxDataPrep : IProposalScxDataPrep
    {
        private readonly IDaypartCache _DaypartCache;
        private readonly IBroadcastAudiencesCache _AudienceCache;
        private readonly IProposalOpenMarketInventoryService _ProposalOpenMarketInventoryService;
        private readonly ISMSClient _SmsClient;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekCache;
        private readonly IDataRepositoryFactory _BroadcastDataRepositoryFactory;

        public ProposalScxDataPrep(IProposalOpenMarketInventoryService proposalOpenMarketInventoryService
            , ISMSClient smsClient
            , IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache
            , IBroadcastAudiencesCache audienceCache, IDaypartCache daypartCache
            , IDataRepositoryFactory broadcastDataRepositoryFactory)
        {
            _ProposalOpenMarketInventoryService = proposalOpenMarketInventoryService;
            _SmsClient = smsClient;
            _MediaMonthAndWeekCache = mediaMonthAndWeekAggregateCache;
            _AudienceCache = audienceCache;
            _DaypartCache = daypartCache;
            _BroadcastDataRepositoryFactory = broadcastDataRepositoryFactory;
        }

        /// <summary>
        /// fyi: open market inventory are assumed to have 1 daypart and 1 program name.  Any more or less will throw exception.
        /// </summary>
        public ScxData GetDataFromProposalDetail(ProposalDto dto, ProposalDetailDto proposalDetailDto)
        {
            var proposalDetailInventory = _ProposalOpenMarketInventoryService.GetInventory(proposalDetailDto.Id.Value, impressionsEquivalized: false);
            var dmaMarketRepo = _BroadcastDataRepositoryFactory.GetDataRepository<IMarketDmaMapRepository>();
            var advertisers = _SmsClient.FindAdvertiserById(dto.AdvertiserId);

            var data = new ScxData
            {
                ProposalDto = dto,
                StartDate = proposalDetailDto.FlightStartDate,
                EndDate = proposalDetailDto.FlightEndDate,
                MarketIds = proposalDetailInventory.Weeks
                    .SelectMany(w => w.Markets.Where(mk => mk.Stations.SelectMany(s => s.Programs).Any(p => p != null && p.Spots > 0)).Select(m => m.MarketId))
                    .Distinct()
                    .ToList(),
                InventoryMarkets = proposalDetailInventory.Markets.ToDictionary(m => m.MarketId, m => new ScxMarketDto {
                    MarketCoverage = m.MarketCoverage,
                    MarketId = m.MarketId,
                    MarketName = m.MarketName,
                    MarketRank = m.MarketRank,
                    MarketSubscribers = m.MarketSubscribers,
                    Stations = m.Stations.Select(x=> new ScxMarketDto.ScxStation
                    {
                        Affiliation = x.Affiliation,
                        CallLetters = x.CallLetters,
                        LegacyCallLetters = x.LegacyCallLetters,
                        StationCode = x.StationCode,
                        Programs = x.Programs.Select(y=> new ScxMarketDto.ScxProgram
                        {
                            Dayparts = y.Dayparts,
                            FlightWeeks = y.FlightWeeks,
                            StationCode = y.StationCode,
                            Genres = y.Genres,
                            ProgramId = y.ProgramId,
                            ProgramNames = y.ProgramNames,
                            ProvidedUnitImpressions = y.ProvidedUnitImpressions,
                            TargetCpm = y.TargetCpm,
                            TargetImpressions = y.TargetImpressions,
                            UnitImpressions = y.UnitImpressions
                        }).ToList()
                    }).ToList()
                }),
                DmaMarketName = dmaMarketRepo.GetMarketMapFromMarketCodes(proposalDetailInventory.Markets.Select(m => m.MarketId).ToList())
                                            .ToDictionary(k => (int)k.market_code, v => v.dma_mapped_value),
                DaypartCode = proposalDetailDto.DaypartCode,
                SpotLength = proposalDetailInventory.DetailSpotLength.ToString(),
                AdvertisersName = advertisers.Display
            };
            
            data.WeekData = _SetProgramWeeks(proposalDetailInventory);
            _SetMarketSurveyData(data, proposalDetailDto);
            _SetDemos(data, proposalDetailDto);
            return data;
        }

        private void _SetMarketSurveyData(ScxData data, ProposalDetailDto proposalDetailDto)
        {
            var bookingMediaMonthId = proposalDetailDto.HutProjectionBookId ?? proposalDetailDto.SingleProjectionBookId.Value;

            var repo = _BroadcastDataRepositoryFactory.GetDataRepository<IRatingForecastRepository>();
            var mediaMonth = _MediaMonthAndWeekCache.GetMediaMonthById(bookingMediaMonthId);
            string mediaMonthInfo = mediaMonth.Abbreviation + mediaMonth.Year.ToString().Substring(2);

            var rawData = repo.GetPlaybackForMarketBy(bookingMediaMonthId, proposalDetailDto.ProjectionPlaybackType);
            data.MarketPlaybackTypes = rawData.Where(d => data.MarketIds.Contains(d.market_code)).ToList();
            data.SurveyData = rawData.ToDictionary(
                k => k.MarketId,
                v => mediaMonthInfo + " DMA Nielsen " + v.PlaybackType.ToString().Replace("Plus", "+"));
        }

        private void _SetDemos(ScxData data, ProposalDetailDto proposalDetailDto)
        {
            var demos = new List<int>
            {
                data.ProposalDto.GuaranteedDemoId
            };
            demos.AddRange(data.ProposalDto.SecondaryDemos);

            var audiencesMappings = _BroadcastDataRepositoryFactory.GetDataRepository<IBroadcastAudienceRepository>()
                        .GetRatingsAudiencesByMaestroAudience(demos).ToList();

            data.Demos = new List<DemoData>();
            int demoIndex = 1;
            foreach (var demo in demos)
            {
                var aud = _AudienceCache.GetAllEntities().Single(a => a.Id == demo);
                var demoData = new DemoData
                {
                    IsPrimaryDemo = demo == demos.First(),
                    Demo = aud,
                    DemoRank = demoIndex,
                    AudienceId = demo,
                    RatingAudienceIds = audiencesMappings.Where(a => a.custom_audience_id == demo).Select(a => a.rating_audience_id).Distinct().ToList()
                };
                data.Demos.Add(demoData);
                demoIndex++;
            }

            _GetDemoUniverseData(data, proposalDetailDto);
            _GetDemoImpressionData(data, proposalDetailDto);
            _GetDemoRatingData(data, proposalDetailDto);
        }

        private void _GetDemoUniverseData(ScxData data, ProposalDetailDto proposalDetailDto)
        {
            var postingBookMonthId = proposalDetailDto.HutProjectionBookId ?? proposalDetailDto.SingleProjectionBookId;
            if (postingBookMonthId == null)
                postingBookMonthId = proposalDetailDto.ShareProjectionBookId.Value;

            foreach (var demo in data.Demos)
            {
                var marketIds = data.MarketIds.Select(m => (short)m).Distinct().ToList();
                var audienceUniversData =
                    _BroadcastDataRepositoryFactory.GetDataRepository<IRatingForecastRepository>()
                        .GetMarketUniverseDataByAudience(postingBookMonthId.Value,
                            demo.RatingAudienceIds.Distinct().ToList(),
                            marketIds,
                            data.MarketPlaybackTypes
                                .Where(pb => marketIds.Contains(pb.market_code))
                                .Select(pb => pb.ForecastPlaybackType.ToString()).Distinct()
                                .ToList());

                demo.MarketPopulations = new Dictionary<short, double>();
                var marketPlaybackTypes = data.MarketPlaybackTypes;
                marketIds.ForEach(mrkId =>
                {
                    double univ = 0;
                    if (audienceUniversData.ContainsKey(mrkId))
                        univ = audienceUniversData[mrkId].Where(
                            u => u.playback_type == marketPlaybackTypes.Single(pb => pb.market_code == mrkId)
                                     .ForecastPlaybackType.ToString()).Sum(u => u.universe1);

                    demo.MarketPopulations.Add(mrkId, univ);
                });
            }
        }

        private void _GetDemoImpressionData(ScxData data, ProposalDetailDto proposalDetailDto)
        {
            var playbackType = proposalDetailDto.ProjectionPlaybackType;

            var repo = _BroadcastDataRepositoryFactory.GetDataRepository<IRatingForecastRepository>();
            bool isSingleBook = proposalDetailDto.SingleProjectionBookId.HasValue;

            var programIds = data.WeekData.Where(wd => wd.InventoryWeek != null)
                .SelectMany(w => w.InventoryWeek.Markets)
                .SelectMany(m => m.Stations)
                .SelectMany(s => s.Programs)
                .Where(p => p != null &&  p.Spots > 0)
                .Select(dd => dd.ProgramId);
            var stations = data.InventoryMarkets
                .SelectMany(pm => pm.Value.Stations)
                .SelectMany(s => s.Programs
                    .Where(p => programIds.Contains(p.ProgramId))
                    .SelectMany(p =>
                        p.Dayparts.Select(pdp => 
                        new ManifestDetailDaypart()
                        {
                            Id = p.ProgramId,
                            LegacyCallLetters =s.LegacyCallLetters
                            ,DisplayDaypart = _DaypartCache.GetDisplayDaypart(pdp.Id) 
                        }))).ToList();

            List<StationImpressions> stationImpressions;
            if (!stations.Any())
                return;

            foreach (var demo in data.Demos)
            {
                var audienceIds = demo.RatingAudienceIds;
                if (isSingleBook)
                {
                    stationImpressions = repo.GetImpressionsDaypart(proposalDetailDto.SingleProjectionBookId.Value
                                               ,audienceIds
                                               ,stations
                                               ,playbackType, BroadcastComposerWebSystemParameter.UseDayByDayImpressions)
                                               .Cast<StationImpressions>()
                                               .ToList();
                }
                else
                {
                    stationImpressions = repo.GetImpressionsDaypart((short)proposalDetailDto.HutProjectionBookId.Value
                                                , (short)proposalDetailDto.ShareProjectionBookId.Value
                                                , audienceIds
                                                , stations
                                                , playbackType, BroadcastComposerWebSystemParameter.UseDayByDayImpressions);
                }
                demo.Impressions = stationImpressions;//.Where(si => data.MarketIds.Contains(si.market_code)).ToList();
            }
        }

        private void _GetDemoRatingData(ScxData data, ProposalDetailDto proposalDetailDto)
        {
            bool isSingleBook = proposalDetailDto.SingleProjectionBookId.HasValue;

            foreach (var demo in data.Demos)
            {
                var programIds = data.WeekData.Where(wd => wd.InventoryWeek != null)
                    .SelectMany(w => w.InventoryWeek.Markets)
                    .SelectMany(m => m.Stations)
                    .SelectMany(s => s.Programs)
                    .Where(p => p != null && p.Spots > 0)
                    .Select(dd => dd.ProgramId);
                var manifestInfo = data.InventoryMarkets
                    .SelectMany(pm => pm.Value.Stations)
                    .SelectMany(s => s.Programs
                        .Where(p => programIds.Contains(p.ProgramId))
                        .SelectMany(p =>
                            p.Dayparts.Select(dp => 
                                new ManifestDetailDaypart() { Id = p.ProgramId,LegacyCallLetters = s.LegacyCallLetters,DisplayDaypart = _DaypartCache.GetDisplayDaypart(dp.Id)})));

                demo.Ratings = new List<Ratingdata>();
                foreach (var impressionData in demo.Impressions)
                {
                    var manifestDetail = manifestInfo.First(m => m.Id == impressionData.Id);

                    Ratingdata demoRating = new Ratingdata()
                    {
                        DaypartId = manifestDetail.DisplayDaypart.Id,
                        Rating = impressionData.Rating  * 100,
                        LegacyCallLetters= impressionData.Legacy_call_letters
                    };
                    demo.Ratings.Add(demoRating);
                }
            }
        }
        
        private List<ScxMarketStationProgramSpotWeek> _SetProgramWeeks(ProposalDetailOpenMarketInventoryDto detail)
        {
            var startDate = detail.DetailFlightStartDate;
            var endDate = detail.DetailFlightEndDate;

            var mediaWeeks = _MediaMonthAndWeekCache.GetMediaWeeksIntersecting(startDate, endDate);

            int weekIndex = 1;
            var weeks = new List<ScxMarketStationProgramSpotWeek>();
            foreach (var mediaWeek in mediaWeeks)
            {
                var weeekData = new ScxMarketStationProgramSpotWeek();
                weeks.Add(weeekData);

                weeekData.MediaWeek = mediaWeek;
                weeekData.StartDate = mediaWeek.StartDate;
                weeekData.WeekNumber = weekIndex++;

                var programWeek = detail.Weeks.FirstOrDefault(w => w.MediaWeekId == mediaWeek.Id);
                if (programWeek == null) // nothing ordered so skip
                    continue;
                weeekData.InventoryWeek = programWeek;
            }
            return weeks;
        }
    }
}



