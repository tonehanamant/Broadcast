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
    
    public partial class station_inventory_manifest_audiences
    {
        public int station_inventory_manifest_id { get; set; }
        public int audience_id { get; set; }
        public Nullable<double> impressions { get; set; }
        public int id { get; set; }
        public Nullable<double> rating { get; set; }
        public bool is_reference { get; set; }
        public Nullable<decimal> cpm { get; set; }
        public Nullable<double> vpvh { get; set; }
        public Nullable<int> share_playback_type { get; set; }
        public Nullable<int> hut_playback_type { get; set; }
    
        public virtual audience audience { get; set; }
        public virtual station_inventory_manifest station_inventory_manifest { get; set; }
    }
}
