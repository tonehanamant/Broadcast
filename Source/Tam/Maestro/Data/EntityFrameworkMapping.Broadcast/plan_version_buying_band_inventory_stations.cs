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
    
    public partial class plan_version_buying_band_inventory_stations
    {
        public plan_version_buying_band_inventory_stations()
        {
            this.plan_version_buying_band_inventory_station_dayparts = new HashSet<plan_version_buying_band_inventory_station_dayparts>();
        }
    
        public int id { get; set; }
        public int plan_version_buying_job_id { get; set; }
        public int posting_type_id { get; set; }
        public int station_id { get; set; }
        public double impressions { get; set; }
        public decimal cost { get; set; }
        public int manifest_weeks_count { get; set; }
    
        public virtual ICollection<plan_version_buying_band_inventory_station_dayparts> plan_version_buying_band_inventory_station_dayparts { get; set; }
        public virtual plan_version_buying_job plan_version_buying_job { get; set; }
        public virtual station station { get; set; }
    }
}