using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities.Isci
{
   public class IsciAdvertiserDto
    {
        public int id { get; set; }
        public string AdvertiserName { get; set; }
        public string Isci { get; set; }
        public int SpotLengthsString { get; set; }
        public string ProductName { get; set; }
    }
}
