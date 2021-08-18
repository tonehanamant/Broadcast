using System.Collections.Generic;

namespace Services.Broadcast.Entities.Isci
{
    public class IsciListItemDto
    {
        public IsciListItemDto()
        {
            Iscis = new List<IsciDto>();
        }
        public string AdvertiserName { get; set; }
        public List<IsciDto> Iscis { get; set; }
    }
}
