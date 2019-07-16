using System;
using System.IO;

namespace Services.Broadcast.Entities.Scx
{
    public class InventoryScxFile
    {
        public MemoryStream ScxStream { get; set; }
        public int DaypartCodeId { get; set; }
        public InventorySource InventorySource { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string UnitName { get; set; }

        public string FileName
        {
            get
            {
                return string.Concat(string.Join("_",
                    new string[] { InventorySource.Name,
                        UnitName,
                        StartDate.ToString("yyyyMMdd"),
                        EndDate.ToString("yyyyMMdd"), }), ".scx");
            }
        }
    }
}
