using System;
using System.Collections.Generic;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.Entities
{
    public class ProposalProgramDto
    {
        //public int Id { get; set; }
        public int ProgramId { get; set; }
        public string ProgramName { get; set; }        
        public int DayPartId { get; set; }
        public LookupDto DayPart { get; set; }
        public List<LookupDto> Genres { get; set; }
        public DisplayScheduleStation Station { get; set; }
        public LookupDto Market { get; set; }
        public int TotalSpots { get; set; }
        public decimal TotalCost { get; set; }
        public double TargetImpressions { get; set; }
        public decimal TargetCpm { get; set; }
        public double TRP { get; set; }
        public decimal HHeCPM { get; set; }
        public double HHImpressions { get; set; }
        public double GRP { get; set; }
        public decimal SpotCost { get; set; }
        public List<ProposalProgramFlightWeek> FlightWeeks { get; set; }
        public double MarketSubscribers { get; set; }
        public double HouseHoldMarketSubscribers { get; set; }
        public double DemoRating { get; set; }
        public double HouseHoldRating { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public double AdditionalAudienceImpressions { get; set; }
        public double AdditionalDemoRating { get; set; }
        public decimal AdditonalAudienceCPM { get; set; }
        public double AdditionalAudienceSubscribers { get; set; }
        public bool IsOverlapping { get; set; }
        public double UnitImpressions { get; set; }
    }
}
