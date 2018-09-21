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
            this.proposal_version_detail_criteria_show_types = new HashSet<proposal_version_detail_criteria_show_types>();
            this.proposal_version_detail_quarters = new HashSet<proposal_version_detail_quarters>();
            this.proposal_version_detail_proprietary_pricing = new HashSet<proposal_version_detail_proprietary_pricing>();
            this.proposal_buy_files = new HashSet<proposal_buy_files>();
            this.open_market_pricing_guide = new HashSet<open_market_pricing_guide>();
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
        public Nullable<int> single_projection_book_id { get; set; }
        public Nullable<int> hut_projection_book_id { get; set; }
        public Nullable<int> share_projection_book_id { get; set; }
        public byte projection_playback_type { get; set; }
        public double open_market_impressions_total { get; set; }
        public decimal open_market_cost_total { get; set; }
        public double proprietary_impressions_total { get; set; }
        public decimal proprietary_cost_total { get; set; }
        public Nullable<int> sequence { get; set; }
        public Nullable<int> posting_book_id { get; set; }
        public Nullable<byte> posting_playback_type { get; set; }
        public double nti_conversion_factor { get; set; }
        public Nullable<double> adjustment_margin { get; set; }
        public Nullable<double> adjustment_rate { get; set; }
        public Nullable<double> adjustment_inflation { get; set; }
        public Nullable<double> goal_impression { get; set; }
        public Nullable<decimal> goal_budget { get; set; }
        public Nullable<decimal> open_market_cpm_min { get; set; }
        public Nullable<decimal> open_market_cpm_max { get; set; }
        public Nullable<int> open_market_unit_cap_per_station { get; set; }
        public Nullable<byte> open_market_cpm_target { get; set; }
    
        public virtual daypart daypart { get; set; }
        public virtual media_months media_months { get; set; }
        public virtual media_months media_months1 { get; set; }
        public virtual media_months media_months2 { get; set; }
        public virtual media_months media_months3 { get; set; }
        public virtual ICollection<proposal_version_detail_criteria_cpm> proposal_version_detail_criteria_cpm { get; set; }
        public virtual ICollection<proposal_version_detail_criteria_genres> proposal_version_detail_criteria_genres { get; set; }
        public virtual ICollection<proposal_version_detail_criteria_programs> proposal_version_detail_criteria_programs { get; set; }
        public virtual ICollection<proposal_version_detail_criteria_show_types> proposal_version_detail_criteria_show_types { get; set; }
        public virtual ICollection<proposal_version_detail_quarters> proposal_version_detail_quarters { get; set; }
        public virtual spot_lengths spot_lengths { get; set; }
        public virtual proposal_versions proposal_versions { get; set; }
        public virtual ICollection<proposal_version_detail_proprietary_pricing> proposal_version_detail_proprietary_pricing { get; set; }
        public virtual ICollection<proposal_buy_files> proposal_buy_files { get; set; }
        public virtual ICollection<open_market_pricing_guide> open_market_pricing_guide { get; set; }
    }
}
