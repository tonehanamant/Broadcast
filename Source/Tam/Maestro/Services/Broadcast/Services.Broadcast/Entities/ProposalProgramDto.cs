using System;
using System.Collections.Generic;
using Services.Broadcast.Repositories;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.Entities
{
    public class ProposalProgramDto
    {
        public int ManifestId { get; set; }
        public List<ManifestDaypartDto> ManifestDayparts { get; set; } 
        public List<LookupDto> DayParts { get; set; }
        public int ManifestDaypartId { get; set; }
        public List<LookupDto> Genres { get; set; }
        public DisplayScheduleStation Station { get; set; }
        public LookupDto Market { get; set; }
        public int TotalSpots { get; set; }
        public decimal TotalCost { get; set; }
        public double TargetImpressions { get; set; }
        public decimal TargetCpm { get; set; }
        public float TRP { get; set; }
        public decimal HHeCPM { get; set; }
        public double HHImpressions { get; set; }
        public float GRP { get; set; }
        public decimal SpotCost { get; set; }
        public List<ProposalProgramFlightWeek> FlightWeeks { get; set; }
        public double MarketSubscribers { get; set; }
        public double HouseHoldMarketSubscribers { get; set; }
        public double DemoRating { get; set; }
        public double HouseHoldRating { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public double AdditionalAudienceImpressions { get; set; }
        public double AdditionalDemoRating { get; set; }
        public decimal AdditonalAudienceCPM { get; set; }
        public double AdditionalAudienceSubscribers { get; set; }
        public bool IsOverlapping { get; set; }
        public double UnitImpressions { get; set; }
        public List<StationInventoryManifestRate> ManifestRates { get; set; }
        public List<StationInventorySpots> Allocations { get; set; }

        public ProposalProgramDto()
        {
            Allocations = new List<StationInventorySpots>();
            ManifestRates = new List<StationInventoryManifestRate>();
            FlightWeeks = new List<ProposalProgramFlightWeek>();
            ManifestDayparts = new List<ManifestDaypartDto>();
            DayParts = new List<LookupDto>();
            Genres = new List<LookupDto>();
        }

        public class ManifestDaypartDto
        {
            public int Id { get; set; }
            public int DaypartId { get; set; }
            public string ProgramName { get; set; }
        }
    }
}
