using System.Collections.Generic;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.Entities.DTO.Program
{
    public class ProgramDto
    {
        public string Name { get; set; }

        public LookupDto Genre { get; set; }

        public string ContentRating { get; set; }
    }
}
