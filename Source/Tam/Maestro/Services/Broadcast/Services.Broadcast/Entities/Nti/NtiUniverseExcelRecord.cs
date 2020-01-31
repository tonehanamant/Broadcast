using Services.Broadcast.Attributes;

namespace Services.Broadcast.Entities
{
    public class NtiUniverseExcelRecord
    {
        [ExcelColumn(1)]
        public int Year { get; set; }

        [ExcelColumn(2)]
        public int Month { get; set; }

        [ExcelColumn(3)]
        public int WeekNumber { get; set; }

        [ExcelColumn(4)]
        public int NtiAudienceId { get; set; }

        [ExcelColumn(5)]
        public string NtiAudienceCode { get; set; }

        [ExcelColumn(6)]
        public double Universe { get; set; }
    }
}
