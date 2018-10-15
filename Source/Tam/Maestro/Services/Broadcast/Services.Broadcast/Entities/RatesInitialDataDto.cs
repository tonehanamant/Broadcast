using Services.Broadcast.Entities.Enums;
using System.Collections.Generic;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.Entities
{
    public class RatesInitialDataDto
    {
        public List<LookupDto> RatingBooks { get; set; }
        public List<LookupDto> PlaybackTypes { get; set; }
        public List<LookupDto> Audiences { get; set; }
        public ProposalEnums.ProposalPlaybackType DefaultPlaybackType { get; set; }   
    }
}
