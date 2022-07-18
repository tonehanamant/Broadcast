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
    
    public partial class audience
    {
        public audience()
        {
            this.audience_audiences = new HashSet<audience_audiences>();
            this.post_file_detail_impressions = new HashSet<post_file_detail_impressions>();
            this.schedule_detail_audiences = new HashSet<schedule_detail_audiences>();
            this.proposal_version_audiences = new HashSet<proposal_version_audiences>();
            this.schedule_audiences = new HashSet<schedule_audiences>();
            this.station_inventory_spot_audiences = new HashSet<station_inventory_spot_audiences>();
            this.affidavit_file_detail_demographics = new HashSet<affidavit_file_detail_demographics>();
            this.proposal_buy_file_detail_audiences = new HashSet<proposal_buy_file_detail_audiences>();
            this.postlog_file_detail_demographics = new HashSet<postlog_file_detail_demographics>();
            this.postlog_client_scrub_audiences = new HashSet<postlog_client_scrub_audiences>();
            this.affidavit_client_scrub_audiences = new HashSet<affidavit_client_scrub_audiences>();
            this.station_inventory_manifest_staging = new HashSet<station_inventory_manifest_staging>();
            this.station_inventory_spot_snapshots = new HashSet<station_inventory_spot_snapshots>();
            this.nti_transmittals_audiences = new HashSet<nti_transmittals_audiences>();
            this.station_inventory_manifest_audiences = new HashSet<station_inventory_manifest_audiences>();
            this.audience_maps = new HashSet<audience_maps>();
            this.plan_version_secondary_audiences = new HashSet<plan_version_secondary_audiences>();
            this.inventory_file_proprietary_header = new HashSet<inventory_file_proprietary_header>();
            this.detection_post_details = new HashSet<detection_post_details>();
            this.nti_universe_audience_mappings = new HashSet<nti_universe_audience_mappings>();
            this.nti_universes = new HashSet<nti_universes>();
            this.vpvhs = new HashSet<vpvh>();
            this.vpvh_quarters = new HashSet<vpvh_quarters>();
            this.vpvh_audience_mappings = new HashSet<vpvh_audience_mappings>();
            this.vpvh_audience_mappings1 = new HashSet<vpvh_audience_mappings>();
            this.inventory_proprietary_summary_station_audiences = new HashSet<inventory_proprietary_summary_station_audiences>();
            this.plan_version_audience_daypart_vpvh = new HashSet<plan_version_audience_daypart_vpvh>();
            this.plan_versions = new HashSet<plan_versions>();
            this.spot_exceptions_out_of_specs = new HashSet<spot_exceptions_out_of_specs>();
        }
    
        public int id { get; set; }
        public byte category_code { get; set; }
        public string sub_category_code { get; set; }
        public Nullable<int> range_start { get; set; }
        public Nullable<int> range_end { get; set; }
        public bool custom { get; set; }
        public string code { get; set; }
        public string name { get; set; }
    
        public virtual ICollection<audience_audiences> audience_audiences { get; set; }
        public virtual ICollection<post_file_detail_impressions> post_file_detail_impressions { get; set; }
        public virtual ICollection<schedule_detail_audiences> schedule_detail_audiences { get; set; }
        public virtual ICollection<proposal_version_audiences> proposal_version_audiences { get; set; }
        public virtual ICollection<schedule_audiences> schedule_audiences { get; set; }
        public virtual ICollection<station_inventory_spot_audiences> station_inventory_spot_audiences { get; set; }
        public virtual nsi_component_audiences nsi_component_audiences { get; set; }
        public virtual ICollection<affidavit_file_detail_demographics> affidavit_file_detail_demographics { get; set; }
        public virtual ICollection<proposal_buy_file_detail_audiences> proposal_buy_file_detail_audiences { get; set; }
        public virtual ICollection<postlog_file_detail_demographics> postlog_file_detail_demographics { get; set; }
        public virtual ICollection<postlog_client_scrub_audiences> postlog_client_scrub_audiences { get; set; }
        public virtual ICollection<affidavit_client_scrub_audiences> affidavit_client_scrub_audiences { get; set; }
        public virtual ICollection<station_inventory_manifest_staging> station_inventory_manifest_staging { get; set; }
        public virtual ICollection<station_inventory_spot_snapshots> station_inventory_spot_snapshots { get; set; }
        public virtual ICollection<nti_transmittals_audiences> nti_transmittals_audiences { get; set; }
        public virtual ICollection<station_inventory_manifest_audiences> station_inventory_manifest_audiences { get; set; }
        public virtual ICollection<audience_maps> audience_maps { get; set; }
        public virtual ICollection<plan_version_secondary_audiences> plan_version_secondary_audiences { get; set; }
        public virtual ICollection<inventory_file_proprietary_header> inventory_file_proprietary_header { get; set; }
        public virtual ICollection<detection_post_details> detection_post_details { get; set; }
        public virtual ICollection<nti_universe_audience_mappings> nti_universe_audience_mappings { get; set; }
        public virtual ICollection<nti_universes> nti_universes { get; set; }
        public virtual ICollection<vpvh> vpvhs { get; set; }
        public virtual ICollection<vpvh_quarters> vpvh_quarters { get; set; }
        public virtual ICollection<vpvh_audience_mappings> vpvh_audience_mappings { get; set; }
        public virtual ICollection<vpvh_audience_mappings> vpvh_audience_mappings1 { get; set; }
        public virtual ICollection<inventory_proprietary_summary_station_audiences> inventory_proprietary_summary_station_audiences { get; set; }
        public virtual ICollection<plan_version_audience_daypart_vpvh> plan_version_audience_daypart_vpvh { get; set; }
        public virtual ICollection<plan_versions> plan_versions { get; set; }
        public virtual ICollection<spot_exceptions_out_of_specs> spot_exceptions_out_of_specs { get; set; }
    }
}
