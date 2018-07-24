using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class BvsFile
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string FileHash { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }

        public List<BvsFileDetail> BvsFileDetails { get; set; } = new List<BvsFileDetail>();
    }
}
