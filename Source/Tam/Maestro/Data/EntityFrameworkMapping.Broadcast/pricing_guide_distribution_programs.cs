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
    
    public partial class pricing_guide_distribution_programs
    {
        public int id { get; set; }
        public int pricing_guide_distribution_id { get; set; }
        public short market_code { get; set; }
        public short station_code { get; set; }
        public int station_inventory_manifest_dayparts_id { get; set; }
        public int daypart_id { get; set; }
        public string program_name { get; set; }
        public decimal blended_cpm { get; set; }
        public int spots { get; set; }
        public double forecasted_impressions_per_spot { get; set; }
        public double station_impressions_per_spot { get; set; }
        public decimal cost_per_spot { get; set; }
    
        public virtual daypart daypart { get; set; }
        public virtual market market { get; set; }
        public virtual station_inventory_manifest_dayparts station_inventory_manifest_dayparts { get; set; }
        public virtual pricing_guide_distributions pricing_guide_distributions { get; set; }
        public virtual station station { get; set; }
    }
}
