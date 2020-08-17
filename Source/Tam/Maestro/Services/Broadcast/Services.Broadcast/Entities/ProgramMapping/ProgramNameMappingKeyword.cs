using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.Entities.ProgramMapping
{
    public class ProgramNameMappingKeyword
    {
        public ProgramNameMappingKeyword()
        {
            ShowType = new LookupDto();
            Genre = new LookupDto();
        }

        public int Id { get; set; }
        public string Keyword { get; set; }
        public LookupDto Genre { get; set; } 
        public LookupDto ShowType { get; set; }
        public string ProgramName { get; set; }
    }
}
