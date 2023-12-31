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
    
    public partial class proposal_version_detail_quarter_weeks
    {
        public proposal_version_detail_quarter_weeks()
        {
            this.station_inventory_spots = new HashSet<station_inventory_spots>();
            this.proposal_version_detail_quarter_week_iscis = new HashSet<proposal_version_detail_quarter_week_iscis>();
            this.affidavit_client_scrubs = new HashSet<affidavit_client_scrubs>();
            this.postlog_client_scrubs = new HashSet<postlog_client_scrubs>();
            this.station_inventory_spot_snapshots = new HashSet<station_inventory_spot_snapshots>();
            this.nti_transmittals_audiences = new HashSet<nti_transmittals_audiences>();
        }
    
        public int id { get; set; }
        public int proposal_version_quarter_id { get; set; }
        public int media_week_id { get; set; }
        public System.DateTime start_date { get; set; }
        public System.DateTime end_date { get; set; }
        public bool is_hiatus { get; set; }
        public int units { get; set; }
        public double impressions_goal { get; set; }
        public decimal cost { get; set; }
        public double open_market_impressions_total { get; set; }
        public decimal open_market_cost_total { get; set; }
        public double proprietary_impressions_total { get; set; }
        public decimal proprietary_cost_total { get; set; }
        public string myevents_report_name { get; set; }
    
        public virtual media_weeks media_weeks { get; set; }
        public virtual proposal_version_detail_quarters proposal_version_detail_quarters { get; set; }
        public virtual ICollection<station_inventory_spots> station_inventory_spots { get; set; }
        public virtual ICollection<proposal_version_detail_quarter_week_iscis> proposal_version_detail_quarter_week_iscis { get; set; }
        public virtual ICollection<affidavit_client_scrubs> affidavit_client_scrubs { get; set; }
        public virtual ICollection<postlog_client_scrubs> postlog_client_scrubs { get; set; }
        public virtual ICollection<station_inventory_spot_snapshots> station_inventory_spot_snapshots { get; set; }
        public virtual ICollection<nti_transmittals_audiences> nti_transmittals_audiences { get; set; }
    }
}
