using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities.Isci
{
    public class IsciSearchDto
    {
        public MediaMonthDto MediaMonth { get; set; }
        public bool WithoutPlansOnly { get; set; }
    }
}
