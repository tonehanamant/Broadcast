namespace Services.Broadcast.Entities
{
    public class ProgramMappingsDto
    {
        public int Id { get; set; }
        public string OriginalProgramName { get; set; }
        public string OfficialProgramName { get; set; }
        public Genre OfficialGenre { get; set; }
        public ShowTypeDto OfficialShowType { get; set; }
    }
}
