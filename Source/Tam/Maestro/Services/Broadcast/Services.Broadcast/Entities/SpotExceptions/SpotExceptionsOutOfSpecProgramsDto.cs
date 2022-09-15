using System.Collections.Generic;

namespace Services.Broadcast.Entities.SpotExceptions
{
    public class SpotExceptionsOutOfSpecProgramsDto
    {
        public SpotExceptionsOutOfSpecProgramsDto()
        {
            this.Genres = new List<string>();
        }
        public string ProgramName { get; set; }
        public List<string> Genres { get; set; }
    }
}
