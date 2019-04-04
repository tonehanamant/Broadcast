using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities
{
    public class InventoryCardFilterDto
    {
        public int? InventorySourceId { get; set; }
        public InventoryCardQuarter Quarter { get; set; }
    }
}
