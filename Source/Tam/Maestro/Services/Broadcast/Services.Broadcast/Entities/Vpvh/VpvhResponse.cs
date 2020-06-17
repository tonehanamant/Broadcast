using OfficeOpenXml;
using Services.Broadcast.Entities.Enums;
using System;

namespace Services.Broadcast.Entities.Vpvh
{
    public class VpvhResponse
    {
        public VpvhResponse()
        {
            LastYearQuarter = new QuarterDto();
            PreviousQuarter = new QuarterDto();
        }

        public int StandardDaypartId { get; set; }

        public int AudienceId { get; set; }

        public DateTime StartingPoint { get; set; }

        public double FourBookAverageVpvh { get; set; }

        public double PreviousQuarterVpvh { get; set; }

        public double LastYearVpvh { get; set; }

        public VpvhTypeEnum VpvhDefaultValue { get; set; }

        public QuarterDto LastYearQuarter { get; set; }

        public QuarterDto PreviousQuarter { get; set; }
    }
}
