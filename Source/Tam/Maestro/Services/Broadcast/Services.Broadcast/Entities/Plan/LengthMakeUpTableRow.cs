﻿using Services.Broadcast.BusinessEngines;

namespace Services.Broadcast.Entities.Plan
{
    public class LengthMakeUpTableRow
    {
        public int SpotLengthId { get; set; }
        public int SpotLengthDuration { get; set; }
        public int GoalPercentage { get; set; }
        public double ImpressionsPercentage { get; set; }
        public decimal Budget { get; set; }
        public double Impressions { get; set; }

        public decimal Cpm => ProposalMath.CalculateCpm(Budget, Impressions);
    }
}
