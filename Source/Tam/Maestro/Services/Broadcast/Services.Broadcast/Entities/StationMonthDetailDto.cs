using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities
{
    public class StationMonthDetailDto
    {
        public int StationId { get; set; }
        public int MediaMonthId { get; set; }
        public string Affiliation { get; set; }
        public short? MarketCode { get; set; }
        public int? DistributorCode { get; set; }
    }
}
