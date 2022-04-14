using System;
using System.IO;
using Services.Broadcast.Extensions;

namespace Services.Broadcast.Entities.Scx
{
    public class PlanPricingScxFile
    {
        private const string FILENAME_TIMESTAMP_FORMAT = "yyyyMMdd_HHmmss";

        public string PlanName { get; set; }
        public DateTime GeneratedTimeStamp { get; set; }
        public MemoryStream ScxStream { get; set; }
        public string spotAllocationModelMode { get; set; }
        public string FileName
        {
            get
            {
                var friendlyPlanName = PlanName.PrepareForUsingInFileName();
                var timestamp = GeneratedTimeStamp.ToString(FILENAME_TIMESTAMP_FORMAT);
                var fileName = $"PlanPricing_{friendlyPlanName}_{timestamp}_{spotAllocationModelMode}.scx";
                return fileName;
            }
        }
    }
}
