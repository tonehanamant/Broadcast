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
    
    public partial class station_inventory_manifest_rates
    {
        public int id { get; set; }
        public int station_inventory_manifest_id { get; set; }
        public int spot_length_id { get; set; }
        public decimal spot_cost { get; set; }
    
        public virtual spot_lengths spot_lengths { get; set; }
        public virtual station_inventory_manifest station_inventory_manifest { get; set; }
    }
}
