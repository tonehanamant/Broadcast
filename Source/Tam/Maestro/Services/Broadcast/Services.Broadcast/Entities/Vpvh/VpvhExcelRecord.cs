using Services.Broadcast.Attributes;

namespace Services.Broadcast.Entities.Vpvh
{
    public class VpvhExcelRecord
    {
        [ExcelColumn(1)]
        public string Audience { get; set; }

        [ExcelColumn(2)]
        public string Quarter { get; set; }

        [ExcelColumn(3)]
        public double AMNews { get; set; }

        [ExcelColumn(4)]
        public double PMNews { get; set; }

        [ExcelColumn(5)]
        public double SynAll { get; set; }
    }
}
