﻿using System;
namespace Services.Broadcast.Entities.SpotExceptions
{
    public class SpotExceptionsOutOfSpecsDto
    {
        public int Id { get; set; }
        public string ReasonCode { get; set; }
        public string ReasonCodeMessage { get; set; }
        public int EstimateId { get; set; }
        public string IsciName { get; set; }
        public int? RecommendedPlanId { get; set; }
        public string ProgramName { get; set; }
        public string StationLegacyCallLetters { get; set; }
        public int? SpotLenthId { get; set; }
        public int? AudienceId { get; set; }
        public string Product { get; set; }
        public DateTime? FlightStartDate { get; set; }
        public DateTime? FlightEndDate { get; set; }
        public int? DaypartId { get; set; }
        public int ProgramDaypartId { get; set; }
        public DateTime ProgramFlightStartDate { get; set; }
        public DateTime ProgramFlightEndDate { get; set; }
        public string ProgramNetwork { get; set; }
        public int? ProgramAudienceId { get; set; }
        public DateTime ProgramAirTime { get; set; }
        public string IngestedBy { get; set; }
        public DateTime IngestedAt { get; set; }
        
    }
}
