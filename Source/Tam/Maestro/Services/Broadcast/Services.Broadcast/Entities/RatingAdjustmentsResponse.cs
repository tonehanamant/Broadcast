using System.Collections.Generic;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.Entities
{
    public class RatingAdjustmentsResponse
    {
        public List<RatingAdjustmentsDto> RatingAdjustments { get; set; } 
        public List<LookupDto> PostingBooks { get; set; }
    }
}
