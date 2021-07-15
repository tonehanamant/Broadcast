using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities.Isci
{
    public class IsciListItemDto
    {
        public string AdvertiserName { get; set; }
        public List<IsciDto> Iscis { get; set; }
       
    }
}
