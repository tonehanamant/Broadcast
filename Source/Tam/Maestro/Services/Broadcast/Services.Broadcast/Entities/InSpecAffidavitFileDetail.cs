using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class InSpecAffidavitFileDetail
    {
        public string Station { get; set; }
        public string Isci { get; set; }
        public string ProgramName { get; set; }
        public int SpotLengthId { get; set; }
        public int AirTime { get; set; }
        public DateTime AirDate { get; set; }
        public string DaypartName { get; set; }
        public Dictionary<int, double> AudienceImpressions { get; set; }
        public int Quarter { get; set; }
        public int Year { get; set; }
        public int AdvertiserId { get; set; }
        public decimal ProposalWeekTotalCost { get; set; }
        public decimal ProposalWeekCost { get; set; }
        public double ProposalWeekTotalImpressionsGoal { get; set; }
        public double ProposalWeekImpressionsGoal { get; set; }
        public decimal ProposalWeekCPM { get; set; }
        public int Units { get; set; }
    }
}
