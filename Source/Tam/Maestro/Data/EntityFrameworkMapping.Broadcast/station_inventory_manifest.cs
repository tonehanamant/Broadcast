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
    
    public partial class station_inventory_manifest
    {
        public station_inventory_manifest()
        {
            this.station_inventory_manifest_dayparts = new HashSet<station_inventory_manifest_dayparts>();
            this.station_inventory_manifest_generation = new HashSet<station_inventory_manifest_generation>();
            this.station_inventory_spots = new HashSet<station_inventory_spots>();
            this.station_inventory_manifest_genres = new HashSet<station_inventory_manifest_genres>();
            this.pricing_guide_distribution_open_market_inventory = new HashSet<pricing_guide_distribution_open_market_inventory>();
            this.station_inventory_manifest_audiences = new HashSet<station_inventory_manifest_audiences>();
            this.station_inventory_manifest_rates = new HashSet<station_inventory_manifest_rates>();
            this.station_inventory_manifest_weeks = new HashSet<station_inventory_manifest_weeks>();
        }
    
        public int id { get; set; }
        public short station_code { get; set; }
        public int spot_length_id { get; set; }
        public Nullable<int> spots_per_week { get; set; }
        public System.DateTime effective_date { get; set; }
        public int inventory_source_id { get; set; }
        public Nullable<int> station_inventory_group_id { get; set; }
        public Nullable<int> file_id { get; set; }
        public Nullable<int> spots_per_day { get; set; }
        public Nullable<System.DateTime> end_date { get; set; }
        public string comment { get; set; }
    
        public virtual inventory_sources inventory_sources { get; set; }
        public virtual spot_lengths spot_lengths { get; set; }
        public virtual station_inventory_group station_inventory_group { get; set; }
        public virtual ICollection<station_inventory_manifest_dayparts> station_inventory_manifest_dayparts { get; set; }
        public virtual ICollection<station_inventory_manifest_generation> station_inventory_manifest_generation { get; set; }
        public virtual ICollection<station_inventory_spots> station_inventory_spots { get; set; }
        public virtual ICollection<station_inventory_manifest_genres> station_inventory_manifest_genres { get; set; }
        public virtual ICollection<pricing_guide_distribution_open_market_inventory> pricing_guide_distribution_open_market_inventory { get; set; }
        public virtual inventory_files inventory_files { get; set; }
        public virtual station station { get; set; }
        public virtual ICollection<station_inventory_manifest_audiences> station_inventory_manifest_audiences { get; set; }
        public virtual ICollection<station_inventory_manifest_rates> station_inventory_manifest_rates { get; set; }
        public virtual ICollection<station_inventory_manifest_weeks> station_inventory_manifest_weeks { get; set; }
    }
}
