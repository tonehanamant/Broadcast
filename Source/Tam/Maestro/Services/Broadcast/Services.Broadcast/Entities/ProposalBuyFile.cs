﻿using Common.Services;
using Services.Broadcast.BusinessEngines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tam.Maestro.Services.ContractInterfaces.AudienceAndRatingsBusinessObjects;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.Entities
{
    public class ProposalBuyFile
    {
        public string FileName { get; set; }
        public string FileHash { get; set; }
        public int EstimateId { get; set; }
        public int ProposalVersionDetailId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<ProposalBuyFileDetail> Details { get; set; }
        public List<string> Errors { get; set; }

        private Dictionary<int, DisplayAudience> FileAudiences { get; set; }
        private StationProcessingEngine StationEngine { get; set; }
        private IMediaMonthAndWeekAggregateCache MediaMonthAndWeekCache { get; set; }
        private IBroadcastAudiencesCache AudienceCache { get; set; }
        private IDaypartCache DaypartCache { get; set; }
        private Dictionary<int, int> SpotLengths { get; set; }

        public ProposalBuyFile() { }

        public ProposalBuyFile(ScxFile scxFile, int estimateId, string fileName, int proposalVersionDetailId,
            List<DisplayBroadcastStation> stations, IMediaMonthAndWeekAggregateCache mediaWeekAndMonthCache, IBroadcastAudiencesCache audienceCache,
            IDaypartCache daypartCache, Dictionary<int, int> spotLengthsDict)
        {
            _CheckFilename(fileName);
            FileName = fileName;
            FileHash = scxFile.FileHash;
            EstimateId = estimateId;
            ProposalVersionDetailId = proposalVersionDetailId;
            StartDate = scxFile.CampaignStartDate;
            EndDate = scxFile.CampaignEndDate;
            StationEngine = new StationProcessingEngine();
            StationEngine.Stations = stations;
            MediaMonthAndWeekCache = mediaWeekAndMonthCache;
            AudienceCache = audienceCache;
            DaypartCache = daypartCache;
            SpotLengths = spotLengthsDict;
            FileAudiences = new Dictionary<int, DisplayAudience>();
            foreach(var scxFileAudience in scxFile.Audiences)
            {
                var broadcastAudience = AudienceCache.FindByAgeRangeAndSubcategory(scxFileAudience.AgeFrom, scxFileAudience.AgeTo, scxFileAudience.Group).SingleOrDefault();
                if(broadcastAudience == null)
                {
                    throw new ApplicationException($"Unable to find demo/audience: {scxFileAudience.Group}{scxFileAudience.AgeFrom}-{scxFileAudience.AgeTo}");
                }
                FileAudiences[scxFileAudience.Rank] = AudienceCache.GetDisplayAudienceById(broadcastAudience.Id);                
            }
            Details = new List<ProposalBuyFileDetail>();
            Errors = new List<string>();
            foreach(var scxDetail in scxFile.Details)
            {
                try
                {
                    Details.Add(new ProposalBuyFileDetail(scxDetail, this));
                }catch(ApplicationException e)
                {
                    this.Errors.Add(e.Message);
                }
            }

        }

        private void _CheckFilename(string fileName)
        {
            if(!fileName.EndsWith(".scx", StringComparison.InvariantCultureIgnoreCase))
            {
                throw new ApplicationException($"Could not import file: {fileName}. Unknown file type");
            }
        }

        public class ProposalBuyFileDetail
        {
            public DisplayBroadcastStation Station { get; set; }
            public decimal SpotCost { get; set; }
            public int TotalSpots { get; set; }
            public decimal TotalCost { get; set; }
            public int SpotLengthId { get; set; }
            public DisplayDaypart Daypart { get; set; }
            public List<ProposalBuyFileDetailWeek> Weeks { get; set; }
            public List<ProposalBuyFileDetailAudience> Audiences { get; set; }

            public ProposalBuyFileDetail() { }

            public ProposalBuyFileDetail(ScxFile.ScxFileDetail detail, ProposalBuyFile proposalBuyFile)
            {
                Station = proposalBuyFile.StationEngine.FindStation(detail.Network);
                if(Station == null)
                {
                    throw new ApplicationException($"Unable to find station {detail.Network}");
                }
                SpotCost = detail.SpotCost;
                TotalSpots = detail.TotalSpots;
                TotalCost = detail.TotalCost;
                int spotLengthId;
                if(!proposalBuyFile.SpotLengths.TryGetValue(detail.SpotLength, out spotLengthId))
                {
                    throw new ApplicationException($"Unable to find spot length {detail.SpotLength}");
                }
                SpotLengthId = spotLengthId;
                var detailDaypart = new DisplayDaypart
                {
                    Monday = detail.Monday,
                    Tuesday = detail.Tuesday,
                    Wednesday = detail.Wednesday,
                    Thursday = detail.Thursday,
                    Friday = detail.Friday,
                    Saturday = detail.Saturday,
                    Sunday = detail.Sunday,
                    StartTime = detail.StartTime,
                    EndTime = detail.EndTime
                };
                detailDaypart.Id = proposalBuyFile.DaypartCache.GetIdByDaypart(detailDaypart);
                Daypart = detailDaypart;
                Weeks = detail.Weeks.Select(w => new ProposalBuyFileDetailWeek(w, proposalBuyFile)).ToList();
                Audiences = detail.Audiences.Select(a => new ProposalBuyFileDetailAudience(a, proposalBuyFile)).ToList();
            }

        }

        public class ProposalBuyFileDetailWeek
        {
            public DisplayMediaWeek MediaWeek { get; set; }
            public int Spots { get; set; }

            public ProposalBuyFileDetailWeek() { }

            public ProposalBuyFileDetailWeek(ScxFile.ScxFileDetailWeek week, ProposalBuyFile proposalBuyFile)
            {
                Spots = week.Spots;
                MediaWeek = proposalBuyFile.MediaMonthAndWeekCache.GetDisplayMediaWeekByFlight(week.StartDate, week.StartDate.AddDays(6)).Single();
            }

        }

        public class ProposalBuyFileDetailAudience
        {
            public int Rank { get; set; }
            public int Population { get; set; }
            public double Impressions { get; set; }
            public DisplayAudience Audience { get; set; }
            public ProposalBuyFileDetailAudience(ScxFile.ScxFileDetailAudience audience, ProposalBuyFile proposalBuyFile)
            {
                Rank = audience.Rank;
                Population = audience.Population;
                Impressions = audience.Impressions;
                Audience = proposalBuyFile.FileAudiences[audience.Rank];
            }

        }
    }
}
