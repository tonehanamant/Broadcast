using Services.Broadcast.Attributes;

namespace Services.Broadcast.Entities
{
    public class ProgramMappingsFileRequestDto
    {
        [ExcelColumn(1)]
        public string OriginalProgramName { get; set; }
        [ExcelColumn(2)]
        public string OfficialProgramName { get; set; }
        [ExcelColumn(3)]
        public string OfficialGenre { get; set; }
        [ExcelColumn(4)]
        public string OfficialShowType { get; set; }
    }
}
