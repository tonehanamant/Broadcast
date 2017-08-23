

create view [dbo].[approved_network_rate_card_books] as 
	select
		nrcb.id, 
		nrcb.name, 
		nrcb.year, 
		nrcb.quarter, 
		nrcb.version, 
		nrcb.media_month_id, 
		nrcb.sales_model_id, 
		nrcb.base_ratings_media_month_id, 
		nrcb.base_coverage_universe_media_month_id, 
		nrcb.approved_by_employee_id, 
		nrcb.date_approved, 
		nrcb.date_created, 
		nrcb.date_last_modified, 
		nrcb.rating_source_id,
		MIN(mm.start_date) start_date,
		MAX(mm.end_date) end_date
	from
		network_rate_card_books nrcb with(nolock)
		join media_months mm with(nolock) on
			(
				mm.year = nrcb.year
				and
				((mm.month - 1) / 3) + 1 = nrcb.quarter
				and
				nrcb.media_month_id is null
			)
			or
			mm.id = nrcb.media_month_id
	where
		nrcb.approved_by_employee_id is not null
	group by
		nrcb.id, 
		nrcb.name, 
		nrcb.year, 
		nrcb.quarter, 
		nrcb.version, 
		nrcb.media_month_id, 
		nrcb.sales_model_id, 
		nrcb.base_ratings_media_month_id, 
		nrcb.base_coverage_universe_media_month_id, 
		nrcb.approved_by_employee_id, 
		nrcb.date_approved, 
		nrcb.date_created, 
		nrcb.date_last_modified, 
		nrcb.rating_source_id;
