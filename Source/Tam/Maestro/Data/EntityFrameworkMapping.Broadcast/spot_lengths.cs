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
    
    public partial class spot_lengths
    {
        public spot_lengths()
        {
            this.post_file_details = new HashSet<post_file_details>();
            this.proposal_version_spot_length = new HashSet<proposal_version_spot_length>();
            this.schedule_details = new HashSet<schedule_details>();
            this.spot_length_cost_multipliers = new HashSet<spot_length_cost_multipliers>();
            this.station_inventory_manifest = new HashSet<station_inventory_manifest>();
            this.spot_tracker_file_details = new HashSet<spot_tracker_file_details>();
            this.proposal_buy_file_details = new HashSet<proposal_buy_file_details>();
            this.station_inventory_manifest_staging = new HashSet<station_inventory_manifest_staging>();
            this.station_inventory_manifest_staging1 = new HashSet<station_inventory_manifest_staging>();
            this.proposal_version_details = new HashSet<proposal_version_details>();
            this.station_inventory_spot_snapshots = new HashSet<station_inventory_spot_snapshots>();
            this.station_inventory_manifest_rates = new HashSet<station_inventory_manifest_rates>();
            this.detection_file_details = new HashSet<detection_file_details>();
            this.plan_version_creative_lengths = new HashSet<plan_version_creative_lengths>();
            this.plan_version_pricing_api_result_spot_frequencies = new HashSet<plan_version_pricing_api_result_spot_frequencies>();
            this.plan_version_buying_api_result_spot_frequencies = new HashSet<plan_version_buying_api_result_spot_frequencies>();
            this.reel_iscis = new HashSet<reel_iscis>();
            this.plan_version_weekly_breakdown = new HashSet<plan_version_weekly_breakdown>();
            this.spot_exceptions_out_of_specs = new HashSet<spot_exceptions_out_of_specs>();
            this.spot_exceptions_recommended_plans = new HashSet<spot_exceptions_recommended_plans>();
        }
    
        public int id { get; set; }
        public int length { get; set; }
        public double delivery_multiplier { get; set; }
        public int order_by { get; set; }
        public bool is_default { get; set; }
    
        public virtual ICollection<post_file_details> post_file_details { get; set; }
        public virtual ICollection<proposal_version_spot_length> proposal_version_spot_length { get; set; }
        public virtual ICollection<schedule_details> schedule_details { get; set; }
        public virtual ICollection<spot_length_cost_multipliers> spot_length_cost_multipliers { get; set; }
        public virtual ICollection<station_inventory_manifest> station_inventory_manifest { get; set; }
        public virtual ICollection<spot_tracker_file_details> spot_tracker_file_details { get; set; }
        public virtual ICollection<proposal_buy_file_details> proposal_buy_file_details { get; set; }
        public virtual ICollection<station_inventory_manifest_staging> station_inventory_manifest_staging { get; set; }
        public virtual ICollection<station_inventory_manifest_staging> station_inventory_manifest_staging1 { get; set; }
        public virtual ICollection<proposal_version_details> proposal_version_details { get; set; }
        public virtual ICollection<station_inventory_spot_snapshots> station_inventory_spot_snapshots { get; set; }
        public virtual ICollection<station_inventory_manifest_rates> station_inventory_manifest_rates { get; set; }
        public virtual ICollection<detection_file_details> detection_file_details { get; set; }
        public virtual ICollection<plan_version_creative_lengths> plan_version_creative_lengths { get; set; }
        public virtual ICollection<plan_version_pricing_api_result_spot_frequencies> plan_version_pricing_api_result_spot_frequencies { get; set; }
        public virtual ICollection<plan_version_buying_api_result_spot_frequencies> plan_version_buying_api_result_spot_frequencies { get; set; }
        public virtual ICollection<reel_iscis> reel_iscis { get; set; }
        public virtual ICollection<plan_version_weekly_breakdown> plan_version_weekly_breakdown { get; set; }
        public virtual ICollection<spot_exceptions_out_of_specs> spot_exceptions_out_of_specs { get; set; }
        public virtual ICollection<spot_exceptions_recommended_plans> spot_exceptions_recommended_plans { get; set; }
    }
}
