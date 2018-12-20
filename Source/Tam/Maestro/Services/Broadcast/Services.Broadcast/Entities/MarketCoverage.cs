using Services.Broadcast.Attributes;

namespace Services.Broadcast.Entities
{
    public class MarketCoverage
    {
        public int MarketCoverageFileId { get; set; }
        [ExcelColumn(1)]
        public int? Rank { get; set; }      
        [ExcelColumn(2)]
        public string Market { get; set; }
        [ExcelColumn(3)]
        public int TVHomes { get; set; }        
        [ExcelColumn(4)]
        public double PercentageOfUS { get; set; }
        public int MarketCode { get; set; }
    }
}
