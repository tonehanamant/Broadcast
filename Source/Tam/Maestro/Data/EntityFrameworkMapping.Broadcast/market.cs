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
    
    public partial class market
    {
        public market()
        {
            this.market_coverages = new HashSet<market_coverages>();
            this.pricing_guide_distribution_open_market_inventory = new HashSet<pricing_guide_distribution_open_market_inventory>();
            this.proposal_version_markets = new HashSet<proposal_version_markets>();
            this.station_inventory_spot_snapshots = new HashSet<station_inventory_spot_snapshots>();
            this.schedules = new HashSet<schedule>();
            this.plan_version_available_markets = new HashSet<plan_version_available_markets>();
            this.plan_version_blackout_markets = new HashSet<plan_version_blackout_markets>();
            this.station_month_details = new HashSet<station_month_details>();
            this.stations = new HashSet<station>();
            this.inventory_proprietary_summary_station_audiences = new HashSet<inventory_proprietary_summary_station_audiences>();
            this.plan_version_daypart_available_markets = new HashSet<plan_version_daypart_available_markets>();
        }
    
        public short market_code { get; set; }
        public string geography_name { get; set; }
    
        public virtual ICollection<market_coverages> market_coverages { get; set; }
        public virtual market_dma_map market_dma_map { get; set; }
        public virtual ICollection<pricing_guide_distribution_open_market_inventory> pricing_guide_distribution_open_market_inventory { get; set; }
        public virtual ICollection<proposal_version_markets> proposal_version_markets { get; set; }
        public virtual ICollection<station_inventory_spot_snapshots> station_inventory_spot_snapshots { get; set; }
        public virtual ICollection<schedule> schedules { get; set; }
        public virtual ICollection<plan_version_available_markets> plan_version_available_markets { get; set; }
        public virtual ICollection<plan_version_blackout_markets> plan_version_blackout_markets { get; set; }
        public virtual ICollection<station_month_details> station_month_details { get; set; }
        public virtual ICollection<station> stations { get; set; }
        public virtual ICollection<inventory_proprietary_summary_station_audiences> inventory_proprietary_summary_station_audiences { get; set; }
        public virtual ICollection<plan_version_daypart_available_markets> plan_version_daypart_available_markets { get; set; }
    }
}
