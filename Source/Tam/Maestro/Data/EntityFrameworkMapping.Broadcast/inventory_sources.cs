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
    
    public partial class inventory_sources
    {
        public inventory_sources()
        {
            this.station_inventory_group = new HashSet<station_inventory_group>();
            this.station_inventory_manifest = new HashSet<station_inventory_manifest>();
        }
    
        public int id { get; set; }
        public string name { get; set; }
        public bool is_active { get; set; }
        public byte inventory_source_type { get; set; }
    
        public virtual ICollection<station_inventory_group> station_inventory_group { get; set; }
        public virtual ICollection<station_inventory_manifest> station_inventory_manifest { get; set; }
    }
}
