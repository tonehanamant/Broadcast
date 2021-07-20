using System.Collections.Generic;

namespace Services.Broadcast.Entities.Isci
{
    public class IsciListItemDto
    {
        public string AdvertiserName { get; set; }
        public List<IsciDto> Iscis { get; set; }
    }
}
