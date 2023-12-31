﻿using Services.Broadcast.Entities.DTO;
using Services.Broadcast.Entities.Enums;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.DTO
{
    public class ClientPostScrubbingProposalDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Advertiser { get; set; }
        public List<ProposalMarketDto> Markets { get; set; } = new List<ProposalMarketDto>();
        public MarketGroupDto BlackoutMarketGroup { get; set; }
        public ProposalEnums.ProposalMarketGroups? MarketGroupId { get; set; }
        public ProposalEnums.ProposalMarketGroups? BlackoutMarketGroupId { get; set; }
        public List<string> SecondaryDemos { get; set; } = new List<string>();
        public string Notes { get; set; }
        public string GuaranteedDemo { get; set; }
        public List<ClientPostScrubbingProposalDetailDto> Details { get; set; } = new List<ClientPostScrubbingProposalDetailDto>();
        public List<ProposalDetailPostScrubbingDto> ClientScrubs { get; set; } = new List<ProposalDetailPostScrubbingDto>();
        public FilterOptions Filters { get; set; }
        public double? CoverageGoal { get; set; }
        public string PostingType { get; set; }
        public bool Equivalized { get; set; }
    }

    public class FilterOptions
    {
        public List<DayOfWeek> DistinctDayOfWeek { get; set; }
        public List<string> DistinctGenres { get; set; }
        public DateTime? WeekStart { get; set; }
        public DateTime? WeekEnd { get; set; }
        public DateTime? DateAiredStart { get; set; }
        public DateTime? DateAiredEnd { get; set; }
        public List<string> DistinctPrograms { get; set; }
        public List<string> DistinctMarkets { get; set; }
        public List<string> DistinctClientIscis { get; set; }
        public List<string> DistinctHouseIscis { get; set; }
        public List<int> DistinctSpotLengths { get; set; }
        public List<string> DistinctAffiliates { get; set; }
        public List<string> DistinctStations { get; set; }
        public List<DateTime> DistinctWeekStarts { get; set; }
        public List<string> DistinctShowTypes { get; set; }
        public List<int> DistinctSequences { get; set; }
        public int? TimeAiredStart { get; set; }
        public int? TimeAiredEnd { get; set; }
        public List<string> DistinctComments { get; set; }
    }
}
