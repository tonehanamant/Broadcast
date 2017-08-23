CREATE VIEW [dbo].[uvw_display_fast_tracks]
AS
	SELECT
		tp.id,
		COUNT(tpp.id) 'num_plans',
		SUM(tpp.total_spots_in_spec) 'total_spots_in_spec',
		SUM(tpp.total_spots_out_of_spec) 'total_spots_out_of_spec',
		tp.number_of_zones_delivering,
		SUM(tpp.post_duration) 'total_post_duration',
		SUM(tpp.aggregation_duration) 'total_aggregation_duration',
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
		MIN(p.advertiser_company_id) 'advertiser_company_id'
	FROM
		tam_post_proposals tpp (NOLOCK)
		JOIN tam_posts tp (NOLOCK) ON tpp.tam_post_id=tp.id
			AND tp.is_deleted=0
		JOIN proposals p (NOLOCK) ON p.id=tpp.posting_plan_proposal_id
		JOIN employees creator (NOLOCK) ON creator.id=tp.created_by_employee_id
		LEFT JOIN employees modifier (NOLOCK) ON modifier.id=tp.modified_by_employee_id
	WHERE
		tpp.post_source_code = 1
    GROUP BY
		tp.id,
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
		creator.firstname,
		creator.lastname,
		modifier.firstname,
		modifier.lastname
