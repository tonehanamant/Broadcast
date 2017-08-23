/****** Object:  View [dbo].[uvw_display_posts]    Script Date: 9/17/2014 2:12:24 PM ******/  
CREATE VIEW [dbo].[uvw_display_posts]  
AS  
 SELECT  
  tp.id,  
  COUNT(tpp.id) 'num_plans',  
  SUM(tpp.total_spots_in_spec) 'total_spots_in_spec',  
  SUM(tpp.total_spots_out_of_spec) 'total_spots_out_of_spec',  
  tp.number_of_zones_delivering,  
  SUM(tpp.post_duration) 'total_post_duration',  
  SUM(tpp.aggregation_duration) 'total_aggregation_duration',  
  tp.is_equivalized,  
  tp.rate_card_type_id 'rate_card_type_id',  
  tp.post_type_code,   
  tp.title,   
  tp.post_setup_agency,   
  tp.post_setup_advertiser,   
  tp.post_setup_product,   
  tp.post_setup_daypart,   
  creator.firstname + ' ' + creator.lastname 'createdby',  
  CASE WHEN modifier.firstname IS NULL THEN '' ELSE modifier.firstname + ' ' + modifier.lastname END 'modifiedby',  
  tp.date_created,   
  tp.date_last_modified,  
  MIN(p.advertiser_company_id) 'advertiser_company_id',  
  dft.total_spots_in_spec 'fast_track_total_spots_in_spec',  
  dft.total_spots_out_of_spec 'fast_track_total_spots_out_of_spec',  
  dft.total_post_duration 'fast_track_total_post_duration',  
  dft.total_aggregation_duration 'fast_track_total_aggregation_duration',  
  tp.produce_monthy_posts,
  tp.produce_quarterly_posts,
  tp.produce_full_posts,
  CAST(MAX(CAST(p.is_msa AS INT)) AS BIT) 'is_msa',  
  MIN(sl.length) 'length',  
  MIN(psm.sales_model_id) 'sales_model_id',  
  MIN(p.start_date)'start_date',   
  MAX(p.end_date)'end_date',  
  rs.id 'rating_source_id',  
  rs.code 'rating_source_code',  
  rs.name 'rating_source_name',  
  rs.default_rating_source_id,  
  tpgd.guaranteed_audience_id 'guaranteed_audience_id',  
        tpgd.guaranteed_audience_code 'guaranteed_audience_code',  
  dmsa.total_spots_in_spec 'msa_total_spots_in_spec',  
  dmsa.total_spots_out_of_spec 'msa_total_spots_out_of_spec',  
  dmsa.total_post_duration 'msa_total_post_duration',  
  dmsa.total_aggregation_duration 'msa_total_aggregation_duration',  
  CASE WHEN mop.tam_post_id IS NOT NULL THEN 1 ELSE 0 END AS 'media_ocean_post',
  dbo.ufn_generate_audit_string(phl.firstname, phl.lastname, phl.transmitted_date) AS media_ocean_last_sent,
  phl.result_status,
  p.is_advanced_tv
 FROM  
  tam_post_proposals tpp (NOLOCK)  
  JOIN tam_posts tp (NOLOCK) ON tpp.tam_post_id=tp.id  
   AND tp.is_deleted=0  
  JOIN proposals p (NOLOCK) ON p.id=tpp.posting_plan_proposal_id  
  JOIN uvw_tam_post_guaranteed_demos tpgd ON tpgd.tam_post_id=tp.id  
  JOIN spot_lengths sl (NOLOCK) ON sl.id=p.default_spot_length_id  
  JOIN employees creator (NOLOCK) ON creator.id=tp.created_by_employee_id  
  LEFT JOIN employees modifier (NOLOCK) ON modifier.id=tp.modified_by_employee_id  
  JOIN uvw_display_fast_tracks dft ON dft.id=tp.id  
  JOIN uvw_display_msa_posts dmsa ON dmsa.id=tp.id  
  JOIN proposal_sales_models psm (NOLOCK) ON psm.proposal_id=p.original_proposal_id  
  JOIN rating_sources rs (NOLOCK) ON rs.id=tp.rating_source_id  
  LEFT JOIN uvw_media_ocean_posts mop ON tp.id = mop.tam_post_id
  LEFT JOIN uvw_post_tecc_log_latest phl ON tp.id = phl.tam_post_id
 WHERE  
  tpp.post_source_code = 0  
    GROUP BY  
  tp.id,  
  tp.is_equivalized,  
  tp.post_type_code,   
  tp.title,   
  tp.post_setup_agency,   
  tp.post_setup_advertiser,   
  tp.post_setup_product,   
  tp.post_setup_daypart,  
  tp.number_of_zones_delivering,  
  tp.rate_card_type_id,  
  tp.date_created,   
  tp.date_last_modified,  
  tp.produce_monthy_posts,
  tp.produce_quarterly_posts,
  tp.produce_full_posts,  
  creator.firstname,  
  creator.lastname,  
  modifier.firstname,  
  modifier.lastname,  
  dft.total_spots_in_spec,  
  dft.total_spots_out_of_spec,  
  dft.total_post_duration,  
  dft.total_aggregation_duration,  
  rs.id,  
  rs.code,  
  rs.name,  
  rs.default_rating_source_id,  
  tpgd.guaranteed_audience_id,  
        tpgd.guaranteed_audience_code,  
  dmsa.total_spots_in_spec,  
  dmsa.total_spots_out_of_spec,  
  dmsa.total_post_duration,  
  dmsa.total_aggregation_duration,
  CASE WHEN mop.tam_post_id IS NOT NULL THEN 1 ELSE 0 END,
  dbo.ufn_generate_audit_string(phl.firstname, phl.lastname, phl.transmitted_date),
  phl.result_status,
  p.is_advanced_tv
