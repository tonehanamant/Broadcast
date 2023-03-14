using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities.Inventory
{
    public class InventoryFileSaveRequestDto: UserInformation
    {
        public string FileName { get; set; }
        public string RawData { get; set; }
    }
}
