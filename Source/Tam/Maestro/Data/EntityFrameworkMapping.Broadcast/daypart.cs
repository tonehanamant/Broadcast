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
    
    public partial class daypart
    {
        public daypart()
        {
            this.days = new HashSet<day>();
            this.schedule_details = new HashSet<schedule_details>();
            this.proposal_buy_file_details = new HashSet<proposal_buy_file_details>();
            this.station_inventory_manifest_staging = new HashSet<station_inventory_manifest_staging>();
            this.proposal_version_details = new HashSet<proposal_version_details>();
            this.pricing_guide_distribution_open_market_inventory = new HashSet<pricing_guide_distribution_open_market_inventory>();
            this.station_inventory_spot_snapshots = new HashSet<station_inventory_spot_snapshots>();
            this.inventory_file_proprietary_header = new HashSet<inventory_file_proprietary_header>();
            this.station_inventory_manifest_dayparts = new HashSet<station_inventory_manifest_dayparts>();
            this.standard_dayparts = new HashSet<standard_dayparts>();
            this.spot_exceptions_out_of_specs = new HashSet<spot_exceptions_out_of_specs>();
            this.spot_exceptions_out_of_specs1 = new HashSet<spot_exceptions_out_of_specs>();
            this.spot_exceptions_recommended_plans = new HashSet<spot_exceptions_recommended_plans>();
        }
    
        public int id { get; set; }
        public int timespan_id { get; set; }
        public string code { get; set; }
        public string name { get; set; }
        public int tier { get; set; }
        public string daypart_text { get; set; }
        public double total_hours { get; set; }
    
        public virtual ICollection<day> days { get; set; }
        public virtual ICollection<schedule_details> schedule_details { get; set; }
        public virtual ICollection<proposal_buy_file_details> proposal_buy_file_details { get; set; }
        public virtual ICollection<station_inventory_manifest_staging> station_inventory_manifest_staging { get; set; }
        public virtual ICollection<proposal_version_details> proposal_version_details { get; set; }
        public virtual ICollection<pricing_guide_distribution_open_market_inventory> pricing_guide_distribution_open_market_inventory { get; set; }
        public virtual ICollection<station_inventory_spot_snapshots> station_inventory_spot_snapshots { get; set; }
        public virtual timespan timespan { get; set; }
        public virtual ICollection<inventory_file_proprietary_header> inventory_file_proprietary_header { get; set; }
        public virtual ICollection<station_inventory_manifest_dayparts> station_inventory_manifest_dayparts { get; set; }
        public virtual ICollection<standard_dayparts> standard_dayparts { get; set; }
        public virtual ICollection<spot_exceptions_out_of_specs> spot_exceptions_out_of_specs { get; set; }
        public virtual ICollection<spot_exceptions_out_of_specs> spot_exceptions_out_of_specs1 { get; set; }
        public virtual ICollection<spot_exceptions_recommended_plans> spot_exceptions_recommended_plans { get; set; }
    }
}
