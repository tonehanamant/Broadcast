using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities
{
    public class InventoryCardInitialData
    {
        public List<InventorySource> InventorySources { get; set; }
        public List<InventoryCardQuarter> Quarters { get; set; }
        public InventoryCardQuarter DefaultQuarter { get; set; }
    }
}
