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
    
    public partial class station_inventory_manifest_dayparts
    {
        public station_inventory_manifest_dayparts()
        {
            this.station_inventory_manifest_daypart_genres = new HashSet<station_inventory_manifest_daypart_genres>();
            this.station_inventory_manifest_daypart_programs = new HashSet<station_inventory_manifest_daypart_programs>();
        }
    
        public int daypart_id { get; set; }
        public int station_inventory_manifest_id { get; set; }
        public int id { get; set; }
        public string program_name { get; set; }
        public Nullable<int> primary_program_id { get; set; }
        public Nullable<int> standard_daypart_id { get; set; }
    
        public virtual daypart daypart { get; set; }
        public virtual station_inventory_manifest station_inventory_manifest { get; set; }
        public virtual ICollection<station_inventory_manifest_daypart_genres> station_inventory_manifest_daypart_genres { get; set; }
        public virtual ICollection<station_inventory_manifest_daypart_programs> station_inventory_manifest_daypart_programs { get; set; }
        public virtual station_inventory_manifest_daypart_programs station_inventory_manifest_daypart_programs1 { get; set; }
        public virtual standard_dayparts standard_dayparts { get; set; }
    }
}
