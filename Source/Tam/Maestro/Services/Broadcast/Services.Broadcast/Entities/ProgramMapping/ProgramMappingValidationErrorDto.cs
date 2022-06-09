namespace Services.Broadcast.Entities.ProgramMapping
{
    public class ProgramMappingValidationErrorDto
    {
        public string RateCardName { get; set; }
        public string MappingProgramName { get; set; }
        public string MappingGenreName { get; set; }
        public string ErrorMessage { get; internal set; }
    }
}
