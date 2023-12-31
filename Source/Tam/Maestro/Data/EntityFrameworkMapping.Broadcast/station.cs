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
    
    public partial class station
    {
        public station()
        {
            this.pricing_guide_distribution_open_market_inventory = new HashSet<pricing_guide_distribution_open_market_inventory>();
            this.proposal_buy_file_details = new HashSet<proposal_buy_file_details>();
            this.station_contacts = new HashSet<station_contacts>();
            this.station_inventory_loaded = new HashSet<station_inventory_loaded>();
            this.station_inventory_manifest = new HashSet<station_inventory_manifest>();
            this.station_inventory_spot_snapshots = new HashSet<station_inventory_spot_snapshots>();
            this.station_mappings = new HashSet<station_mappings>();
            this.station_month_details = new HashSet<station_month_details>();
            this.inventory_proprietary_summary_station_audiences = new HashSet<inventory_proprietary_summary_station_audiences>();
            this.plan_version_buying_band_inventory_stations = new HashSet<plan_version_buying_band_inventory_stations>();
            this.stations_secondary_affiliations = new HashSet<stations_secondary_affiliations>();
        }
    
        public Nullable<int> station_code { get; set; }
        public string station_call_letters { get; set; }
        public string affiliation { get; set; }
        public Nullable<short> market_code { get; set; }
        public string legacy_call_letters { get; set; }
        public string modified_by { get; set; }
        public System.DateTime modified_date { get; set; }
        public int id { get; set; }
        public string rep_firm_name { get; set; }
        public string owner_name { get; set; }
        public bool is_true_ind { get; set; }
    
        public virtual market market { get; set; }
        public virtual ICollection<pricing_guide_distribution_open_market_inventory> pricing_guide_distribution_open_market_inventory { get; set; }
        public virtual ICollection<proposal_buy_file_details> proposal_buy_file_details { get; set; }
        public virtual ICollection<station_contacts> station_contacts { get; set; }
        public virtual ICollection<station_inventory_loaded> station_inventory_loaded { get; set; }
        public virtual ICollection<station_inventory_manifest> station_inventory_manifest { get; set; }
        public virtual ICollection<station_inventory_spot_snapshots> station_inventory_spot_snapshots { get; set; }
        public virtual ICollection<station_mappings> station_mappings { get; set; }
        public virtual ICollection<station_month_details> station_month_details { get; set; }
        public virtual ICollection<inventory_proprietary_summary_station_audiences> inventory_proprietary_summary_station_audiences { get; set; }
        public virtual ICollection<plan_version_buying_band_inventory_stations> plan_version_buying_band_inventory_stations { get; set; }
        public virtual ICollection<stations_secondary_affiliations> stations_secondary_affiliations { get; set; }
    }
}
