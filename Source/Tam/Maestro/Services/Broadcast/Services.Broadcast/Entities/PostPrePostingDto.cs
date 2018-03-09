using System.Collections.Generic;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.Entities
{
    public class PostPrePostingDto
    {
        public List<LookupDto> Demos { get; set; }
        public List<LookupDto> PlaybackTypes { get; set; }
        public List<LookupDto> PostingBooks { get; set; }
    }
}