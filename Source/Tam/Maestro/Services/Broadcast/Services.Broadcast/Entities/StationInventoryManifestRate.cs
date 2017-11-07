using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities
{
    public class StationInventoryManifestRate
    {
        public int Id { get; set; }
        public int SpotLengthId { get; set; }
        public decimal Rate { get; set; }
    }
}
