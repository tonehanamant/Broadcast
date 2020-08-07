using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.QuoteReport
{
    public class QuoteRequestDto
    {
        public DateTime FlightStartDate { get; set; }

        public DateTime FlightEndDate { get; set; }

        public List<DateTime> FlightHiatusDays { get; set; }

        public List<int> FlightDays { get; set; }

        public List<CreativeLength> CreativeLengths { get; set; }

        public List<PlanDaypartDto> Dayparts { get; set; }

        public bool Equivalized { get; set; }

        public PostingTypeEnum PostingType { get; set; }

        public int? HUTBookId { get; set; }

        public int ShareBookId { get; set; }

        public int AudienceId { get; set; }

        public double Margin { get; set; }

        public List<PlanAudienceDto> SecondaryAudiences { get; set; } = new List<PlanAudienceDto>();
    }
}
