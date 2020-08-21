using Services.Broadcast.Attributes;

namespace Services.Broadcast.Entities
{
    public class StationMappingsFileRequestDto
    {
        [ExcelColumn(2)]
        public string CadentCallLetters { get; set; }
        [ExcelColumn(3)]
        public string ExtendedCallLetters { get; set; }
        [ExcelColumn(4)]
        public string SigmaCallLetters { get; set; }
        [ExcelColumn(5)]
        public string NSILegacyCallLetters { get; set; }
        [ExcelColumn(6)]
        public string NSICallLetters { get; set; }
        [ExcelColumn(7)]
        public string Affiliate { get; set; }
        [ExcelColumn(8)]
        public string IsTrueInd { get; set; }
        [ExcelColumn(9)]
        public string OwnershipGroupName { get; set; }
        [ExcelColumn(10)]
        public string SalesGroupName { get; set; }
    }
}