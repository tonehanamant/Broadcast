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
    
    public partial class station_inventory_manifest_daypart_programs
    {
        public int id { get; set; }
        public int station_inventory_manifest_daypart_id { get; set; }
        public string name { get; set; }
        public string show_type { get; set; }
        public int genre_id { get; set; }
        public System.DateTime start_date { get; set; }
        public System.DateTime end_date { get; set; }
        public int start_time { get; set; }
        public int end_time { get; set; }
        public System.DateTime created_date { get; set; }
    
        public virtual genre genre { get; set; }
        public virtual station_inventory_manifest_dayparts station_inventory_manifest_dayparts { get; set; }
    }
}