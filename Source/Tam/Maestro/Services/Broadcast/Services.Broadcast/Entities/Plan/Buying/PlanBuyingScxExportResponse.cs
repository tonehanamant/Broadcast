using Services.Broadcast.Helpers.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Services.Broadcast.Entities.Plan.Buying
{
    public class PlanBuyingScxExportResponse
    {
        public DateTime Generated { get; set; }

        public int PlanId { get; set; }

        public int PlanVersionId { get; set; }

        public int PlanBuyingJobId { get; set; }

        public decimal PlanTargetCpm { get; set; }

        public double? AppliedMargin { get; set; }

        public int? UnallocatedCpmThreshold { get; set; }

        public List<PlanBuyingAllocatedSpot> Allocated { get; set; }

        public List<PlanBuyingAllocatedSpot> Unallocated { get; set; }

        public Stream Stream()
        {
            var json = JsonSerializerHelper.ConvertToJson(this);
            var memoryStream = new MemoryStream();
            var streamWriter = new StreamWriter(memoryStream);
            streamWriter.Write(json);
            streamWriter.Flush();
            memoryStream.Position = 0;
            return memoryStream;
        }

        public string FileName
        {
            get
            {
                var fileName = $"BuyingExport_Plan_{PlanId}_v{PlanBuyingJobId}.scx";
                return fileName;
            }
        }
    }
}