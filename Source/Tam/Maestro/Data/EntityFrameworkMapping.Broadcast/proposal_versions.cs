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
    
    public partial class proposal_versions
    {
        public proposal_versions()
        {
            this.proposal_version_audiences = new HashSet<proposal_version_audiences>();
            this.proposal_version_details = new HashSet<proposal_version_details>();
            this.proposal_version_flight_weeks = new HashSet<proposal_version_flight_weeks>();
            this.proposal_version_markets = new HashSet<proposal_version_markets>();
            this.proposal_version_spot_length = new HashSet<proposal_version_spot_length>();
            this.proposals = new HashSet<proposal>();
        }
    
        public int id { get; set; }
        public int proposal_id { get; set; }
        public short proposal_version { get; set; }
        public Nullable<System.DateTime> start_date { get; set; }
        public Nullable<System.DateTime> end_date { get; set; }
        public Nullable<int> guaranteed_audience_id { get; set; }
        public Nullable<byte> markets { get; set; }
        public string created_by { get; set; }
        public System.DateTime created_date { get; set; }
        public string modified_by { get; set; }
        public System.DateTime modified_date { get; set; }
        public Nullable<decimal> target_budget { get; set; }
        public Nullable<int> target_units { get; set; }
        public double target_impressions { get; set; }
        public string notes { get; set; }
        public Nullable<byte> post_type { get; set; }
        public Nullable<bool> equivalized { get; set; }
        public Nullable<byte> blackout_markets { get; set; }
        public byte status { get; set; }
        public decimal target_cpm { get; set; }
        public double margin { get; set; }
        public decimal cost_total { get; set; }
        public double impressions_total { get; set; }
    
        public virtual ICollection<proposal_version_audiences> proposal_version_audiences { get; set; }
        public virtual ICollection<proposal_version_details> proposal_version_details { get; set; }
        public virtual ICollection<proposal_version_flight_weeks> proposal_version_flight_weeks { get; set; }
        public virtual ICollection<proposal_version_markets> proposal_version_markets { get; set; }
        public virtual ICollection<proposal_version_spot_length> proposal_version_spot_length { get; set; }
        public virtual proposal proposal { get; set; }
        public virtual ICollection<proposal> proposals { get; set; }
    }
}
