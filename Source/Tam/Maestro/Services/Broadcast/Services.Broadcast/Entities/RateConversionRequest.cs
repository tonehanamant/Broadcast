using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities
{
    public class RateConversionRequest
    {
        public Decimal Rate30 { get; set; }
        public int SpotLength { get; set; }

    }
}
