using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using Common.Services;
using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.DTO;
using Services.Broadcast.Entities.OpenMarketInventory;
using Services.Broadcast.Entities.spotcableXML;
using Services.Broadcast.Repositories;
using Tam.Maestro.Common;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Data.EntityFrameworkMapping.ExternalRating;
using Tam.Maestro.Services.Cable.SystemComponentParameters;
using Tam.Maestro.Services.Clients;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.Converters
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

            var data = new ScxData();
            data.ProposalDto = dto;
            data.StartDate = proposalDetailDto.FlightStartDate;
            data.EndDate = proposalDetailDto.FlightEndDate;
            data.MarketIds =
                proposalDetailInventory.Weeks
                    .SelectMany(w => w.Markets.Where(mk => mk.Stations.SelectMany(s => s.Programs).Any(p => p != null && p.Spots > 0)).Select(m => m.MarketId))
                    .Distinct()
                    .ToList();
            data.ProposalInventoryMarkets = proposalDetailInventory.Markets.ToDictionary(m => m.MarketId, m => m);

            var dmaMarketRepo = _BroadcastDataRepositoryFactory.GetDataRepository<IMarketDmaMapRepository>();
            data.ProposalDmaMarketName = dmaMarketRepo
                                            .GetMarketMapFromMarketCodes(proposalDetailInventory.Markets.Select(m => m.MarketId).ToList())
                                            .ToDictionary(k => (int)k.market_code,v => v.dma_mapped_value);
            
            data.DaypartCode = proposalDetailDto.DaypartCode;
            data.SpotLength = proposalDetailInventory.DetailSpotLength.ToString();

            var advertisers = _SmsClient.FindAdvertiserById(dto.AdvertiserId);
            data.AdvertisersName = advertisers.Display;

            data.WeekData = _SetProgramWeeks(data, proposalDetailInventory);
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
            var demos = new List<int>();
            demos.Add(data.ProposalDto.GuaranteedDemoId);
            demos.AddRange(data.ProposalDto.SecondaryDemos);

            var audiencesMappings = _BroadcastDataRepositoryFactory.GetDataRepository<IBroadcastAudienceRepository>()
                        .GetRatingsAudiencesByMaestroAudience(demos).ToList();

            data.Demos = new List<DemoData>();
            int demoIndex = 1;
            foreach (var demo in demos)
            {
                var aud = _AudienceCache.GetAllEntities().Single(a => a.Id == demo);
                var demoData = new DemoData();
                demoData.IsPrimaryDemo = demo == demos.First();
                demoData.Demo = aud;
                demoData.DemoRank = demoIndex;
                demoData.AudienceId = demo;
                demoData.RatingAudienceIds = audiencesMappings
                                                .Where(a => a.custom_audience_id == demo)
                                                .Select(a => a.rating_audience_id)
                                                .Distinct()
                                                .ToList();
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
            var stations = data.ProposalInventoryMarkets
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
                var manifestInfo = data.ProposalInventoryMarkets
                    .SelectMany(pm => pm.Value.Stations)
                    .SelectMany(s => s.Programs
                        .Where(p => programIds.Contains(p.ProgramId))
                        .SelectMany(p =>
                            p.Dayparts.Select(dp => 
                                new ManifestDetailDaypart() { Id = p.ProgramId,LegacyCallLetters = s.LegacyCallLetters,DisplayDaypart = _DaypartCache.GetDisplayDaypart(dp.Id)})));

                demo.Ratings = new List<Ratingdata>();
                foreach (var impressionData in demo.Impressions)
                {
                    var manifestDetail = manifestInfo.First(m => m.Id == impressionData.id);

                    Ratingdata demoRating = new Ratingdata()
                    {
                        DaypartId = manifestDetail.DisplayDaypart.Id,
                        Rating = impressionData.rating  * 100,
                        LegacyCallLetters= impressionData.legacy_call_letters
                    };
                    demo.Ratings.Add(demoRating);
                }

            }
        }


        private List<ScxMarketStationProgramSpotWeek> _SetProgramWeeks(ScxData data,
            ProposalDetailOpenMarketInventoryDto detail)
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

    public class ScxData
    {
        public string DaypartCode { get; set; }
        public string AdvertisersName { get; set; }
        public string SpotLength { get; set; }
        public ProposalDto ProposalDto { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Dictionary<int, string> SurveyData { get; set; }
        public List<MarketPlaybackTypes> MarketPlaybackTypes { get; set; }
        /// <summary>
        /// Contains all possible markets
        /// </summary>
        public Dictionary<int, ProposalInventoryMarketDto> ProposalInventoryMarkets { get; set; }
        public Dictionary<int, string> ProposalDmaMarketName { get; set; }

        /// <summary>
        /// Actual markets with spots
        /// </summary>
        public List<int> MarketIds { get; set; }

        public List<ScxMarketStationProgramSpotWeek> WeekData { get; set; }
        public List<DemoData> Demos { get; set; }
    }

    public class DemoData
    {
        public bool IsPrimaryDemo { get; set; }
        public int DemoRank { get; set; }
        public BroadcastAudience Demo { get; set; }
        public int AudienceId { get; set; }
        public List<int> RatingAudienceIds { get; set; }
        public Dictionary<short, double> MarketPopulations { get; set; }

        public List<StationImpressions> Impressions { get; set; }
        public List<Ratingdata> Ratings { get; set; }
    }

    public class Ratingdata
    {
        public string LegacyCallLetters { get; set; }
        public int DaypartId { get; set; }
        public double Rating { get; set; }
    }

    public class ScxMarketStationProgramSpotWeek
    {
        public MediaWeek MediaWeek { get; set; }
        public int WeekNumber { get; set; }
        public DateTime StartDate { get; set; }
        public ProposalOpenMarketInventoryWeekDto InventoryWeek { get; set; }
    }
}



