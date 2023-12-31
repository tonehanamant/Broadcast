﻿using System;

namespace Services.Broadcast.Entities.DTO
{
    public class ProposalDetailPostScrubbingDto
    {
        public int ScrubbingClientId { get; set; }
        public int? ProposalDetailId { get; set; }
        public DateTime DateAired { get; set; }
        public int TimeAired { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public DateTime WeekStart { get; set; }
        public int SpotLength { get; set; }
        public string ISCI { get; set; }
        public string ProgramName { get; set; }
        public string GenreName { get; set; }
        public string Affiliate { get; set; }
        public string Market { get; set; }
        public string Station { get; set; }
        public bool MatchProgram { get; set; }
        public bool MatchGenre { get; set; }
        public bool MatchMarket { get; set; }
        public bool MatchStation { get; set; }
        public bool MatchTime { get; set; }
        public bool MatchIsciDays { get; set; }
        public string ClientISCI { get; set; }
        public string Comments { get; set; }
        public int? Sequence { get; set; }
        public bool MatchDate { get; set; }
        public string ShowTypeName { get; set; }
        public bool StatusOverride { get; set; }
        public ScrubbingStatus Status { get; set; }
        public bool MatchShowType { get; set; }
        public bool MatchIsci { get; set; }
        public bool SuppliedProgramNameUsed { get; set; }
        public string InventorySource { get; set; }
    }
}
