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
    
    public partial class station_inventory_manifest_weeks
    {
        public int id { get; set; }
        public int station_inventory_manifest_id { get; set; }
        public int media_week_id { get; set; }
        public int spots { get; set; }
    
        public virtual media_weeks media_weeks { get; set; }
        public virtual station_inventory_manifest station_inventory_manifest { get; set; }
    }
}
