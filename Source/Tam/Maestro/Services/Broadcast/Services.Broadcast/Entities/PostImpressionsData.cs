﻿namespace Services.Broadcast.Entities
{
    public class PostImpressionsData
    {
        public int ProposalId { get; set; }
        public double Impressions { get; set; }
        public double? NtiConversionFactor { get; set; }
        public int SpotLengthId { get; set; }
        public int AudienceId { get; set; }
    }
}
