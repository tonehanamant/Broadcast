using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.Vpvh
{
    public class VpvhFile
    {
        public VpvhFile()
        {
            Items = new List<VpvhFileItem>();
        }

        public string FileHash { get; set; }
        public string FileName { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public List<VpvhFileItem> Items { get; set; }
    }
}
