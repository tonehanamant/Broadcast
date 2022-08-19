using System;

namespace Services.Broadcast.Entities
{
    public class MasterProgramsDto
    {
        public int id { get; set; }
        public string Name { get; set; }
        public int ShowTypeId { get; set; }
        public int GenreId { get; set; }
    }
}
