﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class BroadcastContext : DbContext
    {
        public BroadcastContext()
            : base("name=BroadcastContext")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<audience_audiences> audience_audiences { get; set; }
        public virtual DbSet<audience> audiences { get; set; }
        public virtual DbSet<daypart> dayparts { get; set; }
        public virtual DbSet<day> days { get; set; }
        public virtual DbSet<market_dma_map> market_dma_map { get; set; }
        public virtual DbSet<media_months> media_months { get; set; }
        public virtual DbSet<media_weeks> media_weeks { get; set; }
        public virtual DbSet<post_file_demos> post_file_demos { get; set; }
        public virtual DbSet<post_file_detail_impressions> post_file_detail_impressions { get; set; }
        public virtual DbSet<post_file_details> post_file_details { get; set; }
        public virtual DbSet<post_files> post_files { get; set; }
        public virtual DbSet<proposal_version_flight_weeks> proposal_version_flight_weeks { get; set; }
        public virtual DbSet<proposal_version_markets> proposal_version_markets { get; set; }
        public virtual DbSet<proposal_version_spot_length> proposal_version_spot_length { get; set; }
        public virtual DbSet<proposal> proposals { get; set; }
        public virtual DbSet<rating_adjustments> rating_adjustments { get; set; }
        public virtual DbSet<schedule_detail_audiences> schedule_detail_audiences { get; set; }
        public virtual DbSet<schedule_detail_weeks> schedule_detail_weeks { get; set; }
        public virtual DbSet<schedule_details> schedule_details { get; set; }
        public virtual DbSet<schedule_iscis> schedule_iscis { get; set; }
        public virtual DbSet<schedule_restriction_dayparts> schedule_restriction_dayparts { get; set; }
        public virtual DbSet<schedule> schedules { get; set; }
        public virtual DbSet<spot_length_cost_multipliers> spot_length_cost_multipliers { get; set; }
        public virtual DbSet<station_contacts> station_contacts { get; set; }
        public virtual DbSet<system_component_parameters> system_component_parameters { get; set; }
        public virtual DbSet<timespan> timespans { get; set; }
        public virtual DbSet<vw_ccc_daypart> vw_ccc_daypart { get; set; }
        public virtual DbSet<proposal_version_audiences> proposal_version_audiences { get; set; }
        public virtual DbSet<proposal_version_detail_quarters> proposal_version_detail_quarters { get; set; }
        public virtual DbSet<proposal_version_detail_quarter_weeks> proposal_version_detail_quarter_weeks { get; set; }
        public virtual DbSet<proposal_version_detail_criteria_cpm> proposal_version_detail_criteria_cpm { get; set; }
        public virtual DbSet<proposal_version_detail_criteria_genres> proposal_version_detail_criteria_genres { get; set; }
        public virtual DbSet<rep_firms> rep_firms { get; set; }
        public virtual DbSet<schedule_audiences> schedule_audiences { get; set; }
        public virtual DbSet<inventory_sources> inventory_sources { get; set; }
        public virtual DbSet<station_inventory_group> station_inventory_group { get; set; }
        public virtual DbSet<station_inventory_manifest_generation> station_inventory_manifest_generation { get; set; }
        public virtual DbSet<spot_lengths> spot_lengths { get; set; }
        public virtual DbSet<station_inventory_manifest> station_inventory_manifest { get; set; }
        public virtual DbSet<station_inventory_spot_audiences> station_inventory_spot_audiences { get; set; }
        public virtual DbSet<station_inventory_spots> station_inventory_spots { get; set; }
        public virtual DbSet<proposal_version_detail_quarter_week_iscis> proposal_version_detail_quarter_week_iscis { get; set; }
        public virtual DbSet<affidavit_files> affidavit_files { get; set; }
        public virtual DbSet<nsi_component_audiences> nsi_component_audiences { get; set; }
        public virtual DbSet<affidavit_file_detail_problems> affidavit_file_detail_problems { get; set; }
        public virtual DbSet<proposal_version_detail_criteria_programs> proposal_version_detail_criteria_programs { get; set; }
        public virtual DbSet<program_names> program_names { get; set; }
        public virtual DbSet<affidavit_outbound_file_problems> affidavit_outbound_file_problems { get; set; }
        public virtual DbSet<affidavit_outbound_files> affidavit_outbound_files { get; set; }
        public virtual DbSet<proposal_version_detail_criteria_show_types> proposal_version_detail_criteria_show_types { get; set; }
        public virtual DbSet<show_types> show_types { get; set; }
        public virtual DbSet<affidavit_file_problems> affidavit_file_problems { get; set; }
        public virtual DbSet<affidavit_file_detail_demographics> affidavit_file_detail_demographics { get; set; }
        public virtual DbSet<isci_mapping> isci_mapping { get; set; }
        public virtual DbSet<postlog_outbound_file_problems> postlog_outbound_file_problems { get; set; }
        public virtual DbSet<postlog_outbound_files> postlog_outbound_files { get; set; }
        public virtual DbSet<proposal_versions> proposal_versions { get; set; }
        public virtual DbSet<spot_tracker_file_details> spot_tracker_file_details { get; set; }
        public virtual DbSet<spot_tracker_files> spot_tracker_files { get; set; }
        public virtual DbSet<proposal_buy_file_detail_audiences> proposal_buy_file_detail_audiences { get; set; }
        public virtual DbSet<proposal_buy_file_detail_weeks> proposal_buy_file_detail_weeks { get; set; }
        public virtual DbSet<proposal_buy_file_details> proposal_buy_file_details { get; set; }
        public virtual DbSet<proposal_buy_files> proposal_buy_files { get; set; }
        public virtual DbSet<market_coverages> market_coverages { get; set; }
        public virtual DbSet<postlog_file_detail_demographics> postlog_file_detail_demographics { get; set; }
        public virtual DbSet<postlog_file_detail_problems> postlog_file_detail_problems { get; set; }
        public virtual DbSet<postlog_file_problems> postlog_file_problems { get; set; }
        public virtual DbSet<postlog_files> postlog_files { get; set; }
        public virtual DbSet<isci_blacklist> isci_blacklist { get; set; }
        public virtual DbSet<postlog_client_scrub_audiences> postlog_client_scrub_audiences { get; set; }
        public virtual DbSet<station_inventory_manifest_genres> station_inventory_manifest_genres { get; set; }
        public virtual DbSet<station_inventory_manifest_daypart_genres> station_inventory_manifest_daypart_genres { get; set; }
        public virtual DbSet<affidavit_client_scrubs> affidavit_client_scrubs { get; set; }
        public virtual DbSet<affidavit_client_scrub_audiences> affidavit_client_scrub_audiences { get; set; }
        public virtual DbSet<postlog_client_scrubs> postlog_client_scrubs { get; set; }
        public virtual DbSet<postlog_file_details> postlog_file_details { get; set; }
        public virtual DbSet<affidavit_file_details> affidavit_file_details { get; set; }
        public virtual DbSet<station_inventory_manifest_staging> station_inventory_manifest_staging { get; set; }
        public virtual DbSet<pricing_guide_distribution_proprietary_inventory> pricing_guide_distribution_proprietary_inventory { get; set; }
        public virtual DbSet<proposal_version_details> proposal_version_details { get; set; }
        public virtual DbSet<nti_transmittals_file_problems> nti_transmittals_file_problems { get; set; }
        public virtual DbSet<nti_transmittals_file_report_ratings> nti_transmittals_file_report_ratings { get; set; }
        public virtual DbSet<nti_transmittals_file_reports> nti_transmittals_file_reports { get; set; }
        public virtual DbSet<nti_transmittals_files> nti_transmittals_files { get; set; }
        public virtual DbSet<pricing_guide_distribution_open_market_inventory> pricing_guide_distribution_open_market_inventory { get; set; }
        public virtual DbSet<pricing_guide_distributions> pricing_guide_distributions { get; set; }
        public virtual DbSet<station_inventory_spot_snapshots> station_inventory_spot_snapshots { get; set; }
        public virtual DbSet<nti_transmittals_audiences> nti_transmittals_audiences { get; set; }
        public virtual DbSet<market_coverage_files> market_coverage_files { get; set; }
        public virtual DbSet<station_inventory_loaded> station_inventory_loaded { get; set; }
        public virtual DbSet<inventory_files> inventory_files { get; set; }
        public virtual DbSet<inventory_file_problems> inventory_file_problems { get; set; }
        public virtual DbSet<market> markets { get; set; }
        public virtual DbSet<station_inventory_manifest_audiences> station_inventory_manifest_audiences { get; set; }
        public virtual DbSet<station_inventory_manifest_rates> station_inventory_manifest_rates { get; set; }
        public virtual DbSet<station_inventory_manifest_weeks> station_inventory_manifest_weeks { get; set; }
        public virtual DbSet<inventory_file_ratings_jobs> inventory_file_ratings_jobs { get; set; }
        public virtual DbSet<station_inventory_manifest_weeks_history> station_inventory_manifest_weeks_history { get; set; }
        public virtual DbSet<inventory_source_logos> inventory_source_logos { get; set; }
        public virtual DbSet<audience_maps> audience_maps { get; set; }
        public virtual DbSet<scx_generation_job_units> scx_generation_job_units { get; set; }
        public virtual DbSet<campaign> campaigns { get; set; }
        public virtual DbSet<campaign_summaries> campaign_summaries { get; set; }
        public virtual DbSet<inventory_file_ratings_job_notes> inventory_file_ratings_job_notes { get; set; }
        public virtual DbSet<genre_mappings> genre_mappings { get; set; }
        public virtual DbSet<plan_version_available_markets> plan_version_available_markets { get; set; }
        public virtual DbSet<plan_version_blackout_markets> plan_version_blackout_markets { get; set; }
        public virtual DbSet<plan_version_flight_hiatus_days> plan_version_flight_hiatus_days { get; set; }
        public virtual DbSet<plan_version_secondary_audiences> plan_version_secondary_audiences { get; set; }
        public virtual DbSet<plan_version_summaries> plan_version_summaries { get; set; }
        public virtual DbSet<plan> plans { get; set; }
        public virtual DbSet<plan_version_summary_quarters> plan_version_summary_quarters { get; set; }
        public virtual DbSet<plan_version_daypart_show_type_restrictions> plan_version_daypart_show_type_restrictions { get; set; }
        public virtual DbSet<plan_version_daypart_genre_restrictions> plan_version_daypart_genre_restrictions { get; set; }
        public virtual DbSet<plan_version_daypart_program_restrictions> plan_version_daypart_program_restrictions { get; set; }
        public virtual DbSet<affiliate> affiliates { get; set; }
        public virtual DbSet<plan_version_daypart_affiliate_restrictions> plan_version_daypart_affiliate_restrictions { get; set; }
        public virtual DbSet<inventory_file_proprietary_header> inventory_file_proprietary_header { get; set; }
        public virtual DbSet<inventory_summary_quarter_details> inventory_summary_quarter_details { get; set; }
        public virtual DbSet<plan_version_dayparts> plan_version_dayparts { get; set; }
        public virtual DbSet<scx_generation_job_files> scx_generation_job_files { get; set; }
        public virtual DbSet<scx_generation_jobs> scx_generation_jobs { get; set; }
        public virtual DbSet<station_inventory_manifest_dayparts> station_inventory_manifest_dayparts { get; set; }
        public virtual DbSet<inventory_summary> inventory_summary { get; set; }
        public virtual DbSet<inventory_summary_gap_ranges> inventory_summary_gap_ranges { get; set; }
        public virtual DbSet<inventory_summary_gaps> inventory_summary_gaps { get; set; }
        public virtual DbSet<inventory_summary_quarters> inventory_summary_quarters { get; set; }
        public virtual DbSet<detection_file_details> detection_file_details { get; set; }
        public virtual DbSet<detection_files> detection_files { get; set; }
        public virtual DbSet<detection_map_types> detection_map_types { get; set; }
        public virtual DbSet<detection_maps> detection_maps { get; set; }
        public virtual DbSet<detection_post_details> detection_post_details { get; set; }
        public virtual DbSet<plan_version_pricing_result_spots> plan_version_pricing_result_spots { get; set; }
        public virtual DbSet<shared_folder_files> shared_folder_files { get; set; }
        public virtual DbSet<nti_universe_audience_mappings> nti_universe_audience_mappings { get; set; }
        public virtual DbSet<nti_universe_details> nti_universe_details { get; set; }
        public virtual DbSet<nti_universe_headers> nti_universe_headers { get; set; }
        public virtual DbSet<nti_universes> nti_universes { get; set; }
        public virtual DbSet<inventory_programs_by_file_job_notes> inventory_programs_by_file_job_notes { get; set; }
        public virtual DbSet<inventory_programs_by_file_jobs> inventory_programs_by_file_jobs { get; set; }
        public virtual DbSet<inventory_programs_by_source_job_notes> inventory_programs_by_source_job_notes { get; set; }
        public virtual DbSet<inventory_programs_by_source_jobs> inventory_programs_by_source_jobs { get; set; }
        public virtual DbSet<nti_to_nsi_conversion_rates> nti_to_nsi_conversion_rates { get; set; }
        public virtual DbSet<plan_version_flight_days> plan_version_flight_days { get; set; }
        public virtual DbSet<station_mappings> station_mappings { get; set; }
        public virtual DbSet<station_month_details> station_month_details { get; set; }
        public virtual DbSet<plan_versions> plan_versions { get; set; }
        public virtual DbSet<plan_version_creative_lengths> plan_version_creative_lengths { get; set; }
        public virtual DbSet<program_name_mappings> program_name_mappings { get; set; }
        public virtual DbSet<inventory_export_jobs> inventory_export_jobs { get; set; }
        public virtual DbSet<vpvh_files> vpvh_files { get; set; }
        public virtual DbSet<vpvh> vpvhs { get; set; }
        public virtual DbSet<plan_version_pricing_band_details> plan_version_pricing_band_details { get; set; }
        public virtual DbSet<program_sources> program_sources { get; set; }
        public virtual DbSet<genre> genres { get; set; }
        public virtual DbSet<station_inventory_manifest_daypart_programs> station_inventory_manifest_daypart_programs { get; set; }
        public virtual DbSet<vpvh_quarters> vpvh_quarters { get; set; }
        public virtual DbSet<vpvh_audience_mappings> vpvh_audience_mappings { get; set; }
        public virtual DbSet<program_name_exceptions> program_name_exceptions { get; set; }
        public virtual DbSet<plan_version_pricing_market_details> plan_version_pricing_market_details { get; set; }
        public virtual DbSet<plan_version_pricing_station_details> plan_version_pricing_station_details { get; set; }
        public virtual DbSet<plan_version_pricing_api_results> plan_version_pricing_api_results { get; set; }
        public virtual DbSet<plan_version_pricing_bands> plan_version_pricing_bands { get; set; }
        public virtual DbSet<plan_version_pricing_markets> plan_version_pricing_markets { get; set; }
        public virtual DbSet<plan_version_pricing_stations> plan_version_pricing_stations { get; set; }
        public virtual DbSet<plan_version_pricing_api_result_spot_frequencies> plan_version_pricing_api_result_spot_frequencies { get; set; }
        public virtual DbSet<plan_version_pricing_api_result_spots> plan_version_pricing_api_result_spots { get; set; }
        public virtual DbSet<plan_version_pricing_job> plan_version_pricing_job { get; set; }
        public virtual DbSet<plan_version_pricing_parameters> plan_version_pricing_parameters { get; set; }
        public virtual DbSet<plan_version_buying_api_result_spot_frequencies> plan_version_buying_api_result_spot_frequencies { get; set; }
        public virtual DbSet<plan_version_buying_api_result_spots> plan_version_buying_api_result_spots { get; set; }
        public virtual DbSet<plan_version_buying_api_results> plan_version_buying_api_results { get; set; }
        public virtual DbSet<plan_version_buying_band_details> plan_version_buying_band_details { get; set; }
        public virtual DbSet<plan_version_buying_job> plan_version_buying_job { get; set; }
        public virtual DbSet<plan_version_buying_parameters> plan_version_buying_parameters { get; set; }
        public virtual DbSet<plan_version_buying_result_spots> plan_version_buying_result_spots { get; set; }
        public virtual DbSet<plan_version_buying_station_details> plan_version_buying_station_details { get; set; }
        public virtual DbSet<program_name_mapping_keywords> program_name_mapping_keywords { get; set; }
        public virtual DbSet<show_type_mappings> show_type_mappings { get; set; }
        public virtual DbSet<plan_version_buying_ownership_group_details> plan_version_buying_ownership_group_details { get; set; }
        public virtual DbSet<inventory_proprietary_daypart_program_mappings> inventory_proprietary_daypart_program_mappings { get; set; }
        public virtual DbSet<plan_version_buying_results> plan_version_buying_results { get; set; }
        public virtual DbSet<plan_version_buying_rep_firm_details> plan_version_buying_rep_firm_details { get; set; }
        public virtual DbSet<plan_version_buying_market_details> plan_version_buying_market_details { get; set; }
        public virtual DbSet<station> stations { get; set; }
        public virtual DbSet<inventory_proprietary_daypart_programs> inventory_proprietary_daypart_programs { get; set; }
        public virtual DbSet<inventory_proprietary_summary> inventory_proprietary_summary { get; set; }
        public virtual DbSet<plan_version_buying_parameter_inventory_proprietary_summaries> plan_version_buying_parameter_inventory_proprietary_summaries { get; set; }
        public virtual DbSet<plan_version_pricing_parameter_inventory_proprietary_summaries> plan_version_pricing_parameter_inventory_proprietary_summaries { get; set; }
        public virtual DbSet<inventory_proprietary_summary_station_audiences> inventory_proprietary_summary_station_audiences { get; set; }
        public virtual DbSet<standard_dayparts> standard_dayparts { get; set; }
        public virtual DbSet<plan_version_pricing_results_dayparts> plan_version_pricing_results_dayparts { get; set; }
        public virtual DbSet<plan_version_pricing_results> plan_version_pricing_results { get; set; }
        public virtual DbSet<reel_isci_advertiser_name_references> reel_isci_advertiser_name_references { get; set; }
        public virtual DbSet<reel_isci_products> reel_isci_products { get; set; }
        public virtual DbSet<reel_iscis> reel_iscis { get; set; }
        public virtual DbSet<export_unmapped_program_names_jobs> export_unmapped_program_names_jobs { get; set; }
        public virtual DbSet<reel_isci_ingest_jobs> reel_isci_ingest_jobs { get; set; }
        public virtual DbSet<plan_version_buying_result_spot_stations> plan_version_buying_result_spot_stations { get; set; }
        public virtual DbSet<plan_version_buying_band_station_dayparts> plan_version_buying_band_station_dayparts { get; set; }
        public virtual DbSet<plan_version_buying_band_stations> plan_version_buying_band_stations { get; set; }
        public virtual DbSet<spot_exceptions_ingest_jobs> spot_exceptions_ingest_jobs { get; set; }
        public virtual DbSet<spot_exceptions_out_of_spec_decisions> spot_exceptions_out_of_spec_decisions { get; set; }
        public virtual DbSet<spot_exceptions_recommended_plan_decision> spot_exceptions_recommended_plan_decision { get; set; }
        public virtual DbSet<spot_exceptions_recommended_plan_details> spot_exceptions_recommended_plan_details { get; set; }
        public virtual DbSet<plan_version_weekly_breakdown> plan_version_weekly_breakdown { get; set; }
        public virtual DbSet<spot_exceptions_recommended_plans> spot_exceptions_recommended_plans { get; set; }
        public virtual DbSet<spot_exceptions_out_of_spec_reason_codes> spot_exceptions_out_of_spec_reason_codes { get; set; }
        public virtual DbSet<custom_daypart_organizations> custom_daypart_organizations { get; set; }
        public virtual DbSet<plan_version_daypart_customizations> plan_version_daypart_customizations { get; set; }
        public virtual DbSet<staged_unposted_no_plan> staged_unposted_no_plan { get; set; }
        public virtual DbSet<staged_unposted_no_reel_roster> staged_unposted_no_reel_roster { get; set; }
        public virtual DbSet<spot_exceptions_unposted_no_plan> spot_exceptions_unposted_no_plan { get; set; }
        public virtual DbSet<spot_exceptions_unposted_no_reel_roster> spot_exceptions_unposted_no_reel_roster { get; set; }
        public virtual DbSet<plan_version_daypart_goals> plan_version_daypart_goals { get; set; }
        public virtual DbSet<spot_exceptions_out_of_specs> spot_exceptions_out_of_specs { get; set; }
        public virtual DbSet<plan_version_daypart_available_markets> plan_version_daypart_available_markets { get; set; }
        public virtual DbSet<plan_version_daypart_flight_days> plan_version_daypart_flight_days { get; set; }
        public virtual DbSet<plan_version_daypart_flight_hiatus_days> plan_version_daypart_flight_hiatus_days { get; set; }
        public virtual DbSet<plan_version_daypart_weekly_breakdown> plan_version_daypart_weekly_breakdown { get; set; }
        public virtual DbSet<plan_iscis> plan_iscis { get; set; }
        public virtual DbSet<plan_version_audience_daypart_vpvh> plan_version_audience_daypart_vpvh { get; set; }
        public virtual DbSet<staged_recommended_plan_details> staged_recommended_plan_details { get; set; }
        public virtual DbSet<staged_recommended_plans> staged_recommended_plans { get; set; }
        public virtual DbSet<plan_version_buying_band_inventory_station_dayparts> plan_version_buying_band_inventory_station_dayparts { get; set; }
        public virtual DbSet<plan_version_buying_band_inventory_stations> plan_version_buying_band_inventory_stations { get; set; }
        public virtual DbSet<staged_out_of_specs> staged_out_of_specs { get; set; }
    }
}
