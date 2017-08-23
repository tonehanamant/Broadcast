CREATE VIEW [dbo].[uvw_all_display_proposals]
AS
	SELECT
		p.id,
		p.original_proposal_id,
		p.version_number,
		ps.name 'status',
		p.total_gross_cost,
		adv.name 'advertiser',
		pr.name 'product',
		agy.name 'agency',
		p.name 'title',
		(e.firstname + ' ' + e.lastname) 'salesperson',
		p.flight_text,
		a.name 'primary_audience',
		p.include_on_availability_planner,
		p.date_created,
		p.date_last_modified,
		p.proposal_status_id,
		CASE WHEN p.is_audience_deficiency_unit_schedule IS NULL THEN CAST(0 AS BIT) ELSE p.is_audience_deficiency_unit_schedule END 'is_audience_deficiency_unit_schedule',
		p.rating_source_id,
		p.start_date,
		p.end_date,
		p.rate_card_type_id,
		p.total_spots,
		d.daypart_text 'primary_daypart',
		CASE WHEN pp.proposal_id IS NULL THEN
				CAST(0 AS BIT)
			ELSE 
				CAST(1 AS BIT)
		END 'has_been_posted',
		po.proposal_status_id 'original_proposal_status_id',
		p.number_of_materials,
		mm.media_month,
		psm.sales_model_id,
		CASE WHEN p.is_msa IS NULL THEN CAST(0 AS BIT) ELSE p.is_msa END 'is_msa',
		CASE WHEN ftp.proposal_id IS NULL THEN
				CAST(0 AS BIT)
			ELSE 
				CAST(1 AS BIT)
		END 'has_been_fast_tracked',
		p.posting_media_month_id,
		sl.[length],
		a.id 'primary_audience_id'
	FROM 
		proposals p							(NOLOCK)
		LEFT JOIN products pr				(NOLOCK) ON pr.id=p.product_id 
		LEFT JOIN companies adv				(NOLOCK) ON adv.id=p.advertiser_company_id 
		LEFT JOIN companies agy				(NOLOCK) ON agy.id=p.agency_company_id 
		LEFT JOIN employees e				(NOLOCK) ON e.id=p.salesperson_employee_id 
		LEFT JOIN proposal_statuses	ps		(NOLOCK) ON ps.id=p.proposal_status_id 
		LEFT JOIN proposal_audiences pa		(NOLOCK) ON pa.proposal_id=p.id AND pa.ordinal=1
		LEFT JOIN audiences a				(NOLOCK) ON a.id=pa.audience_id
		LEFT JOIN dayparts d				(NOLOCK) ON d.id=p.primary_daypart_id
		LEFT JOIN proposals po				(NOLOCK) ON po.id=p.original_proposal_id
		LEFT JOIN media_months mm			(NOLOCK) ON mm.id=p.posting_media_month_id
		LEFT JOIN uvw_posted_proposals pp			 ON pp.proposal_id=p.id
		LEFT JOIN uvw_fast_tracked_proposals ftp	 ON ftp.proposal_id=p.id
		LEFT JOIN proposal_sales_models psm (NOLOCK) ON psm.proposal_id=p.id
		LEFT JOIN spot_lengths sl			(NOLOCK) ON sl.id=p.default_spot_length_id


GO
EXECUTE sp_addextendedproperty @name = N'primary_key', @value = N'id', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'VIEW', @level1name = N'uvw_all_display_proposals';

