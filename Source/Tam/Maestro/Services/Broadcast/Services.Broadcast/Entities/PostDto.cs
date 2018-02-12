using System.Collections.Generic;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.Entities
{
    public class PostDto
    {
        public List<LookupDto> Demos { get; set; }
        public List<LookupDto> PlaybackTypes { get; set; }
        public List<LookupDto> PostingBooks { get; set; }
    }
}