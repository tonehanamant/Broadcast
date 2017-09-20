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
    
    public partial class proposal_version_details
    {
        public proposal_version_details()
        {
            this.proposal_version_detail_criteria_cpm = new HashSet<proposal_version_detail_criteria_cpm>();
            this.proposal_version_detail_criteria_genres = new HashSet<proposal_version_detail_criteria_genres>();
            this.proposal_version_detail_criteria_programs = new HashSet<proposal_version_detail_criteria_programs>();
            this.proposal_version_detail_quarters = new HashSet<proposal_version_detail_quarters>();
        }
    
        public int id { get; set; }
        public int proposal_version_id { get; set; }
        public int spot_length_id { get; set; }
        public int daypart_id { get; set; }
        public string daypart_code { get; set; }
        public System.DateTime start_date { get; set; }
        public System.DateTime end_date { get; set; }
        public Nullable<int> units_total { get; set; }
        public double impressions_total { get; set; }
        public Nullable<decimal> cost_total { get; set; }
        public bool adu { get; set; }
        public Nullable<int> single_posting_book_id { get; set; }
        public Nullable<int> hut_posting_book_id { get; set; }
        public Nullable<int> share_posting_book_id { get; set; }
        public Nullable<byte> playback_type { get; set; }
        public double open_market_impressions_total { get; set; }
        public decimal open_market_cost_total { get; set; }
        public double proprietary_impressions_total { get; set; }
        public decimal proprietary_cost_total { get; set; }
    
        public virtual ICollection<proposal_version_detail_criteria_cpm> proposal_version_detail_criteria_cpm { get; set; }
        public virtual ICollection<proposal_version_detail_criteria_genres> proposal_version_detail_criteria_genres { get; set; }
        public virtual ICollection<proposal_version_detail_criteria_programs> proposal_version_detail_criteria_programs { get; set; }
        public virtual ICollection<proposal_version_detail_quarters> proposal_version_detail_quarters { get; set; }
        public virtual daypart daypart { get; set; }
        public virtual media_months media_months { get; set; }
        public virtual media_months media_months1 { get; set; }
        public virtual media_months media_months2 { get; set; }
        public virtual spot_lengths spot_lengths { get; set; }
        public virtual proposal_versions proposal_versions { get; set; }
    }
}
