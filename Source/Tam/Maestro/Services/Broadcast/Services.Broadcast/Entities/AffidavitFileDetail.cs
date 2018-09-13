using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class AffidavitFileDetail
    {
        public long Id { get; set; }
        public int AffidavitFileId { get; set; }
        public string Station { get; set; }
        public DateTime OriginalAirDate { get; set; }
        public DateTime AdjustedAirDate { get; set; }
        public int AirTime { get; set; }
        public int SpotLengthId { get; set; }
        public string Isci { get; set; }
        public string ProgramName { get; set; }
        public string Genre { get; set; }
        public string LeadinGenre { get; set; }
        public string LeadinProgramName { get; set; }
        public string LeadoutGenre { get; set; }        
        public string LeadoutProgramName { get; set; }
        public string Market { get; set; }
        public int? EstimateId { get; set; }
        public int InventorySource { get; set; }
        public double? SpotCost { get; set; }
        public string Affiliate { get; set; }
        public List<WWTVDemographics> Demographics { get; set; } = new List<WWTVDemographics>();
        public List<AffidavitClientScrub> AffidavitClientScrubs { get; set; } = new List<AffidavitClientScrub>();
        public List<FileDetailProblem> AffidavitFileDetailProblems { get; set; } = new List<FileDetailProblem>();
        public string ShowType { get; set; }
        public string LeadInShowType { get; set; }
        public string LeadOutShowType { get; set; }
        public int LeadOutStartTime { get; set; }
        public int LeadInEndTime { get; set; }
        public bool Archived { get; set; }
        public string MappedIsci { get; set; }        
    }
}
