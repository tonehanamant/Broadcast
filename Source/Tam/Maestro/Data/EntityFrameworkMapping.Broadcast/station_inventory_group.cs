//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EntityFrameworkMapping.Broadcast
{
    using System;
    using System.Collections.Generic;
    
    public partial class station_inventory_group
    {
        public station_inventory_group()
        {
            this.station_inventory_manifest = new HashSet<station_inventory_manifest>();
        }
    
        public int id { get; set; }
        public string name { get; set; }
        public string daypart_code { get; set; }
        public byte slot_number { get; set; }
        public int inventory_source_id { get; set; }
    
        public virtual inventory_sources inventory_sources { get; set; }
        public virtual ICollection<station_inventory_manifest> station_inventory_manifest { get; set; }
    }
}
