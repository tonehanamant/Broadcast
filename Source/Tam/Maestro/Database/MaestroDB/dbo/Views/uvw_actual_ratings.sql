CREATE view [dbo].[uvw_actual_ratings] as 
	select	
		r.rating_category_id,
		r.base_media_month_id 'media_month_id',
		r.nielsen_network_id,
		r.daypart_id,
		r.audience_id,
		r.audience_usage,
		r.tv_usage
	from
		ratings r
	where
		r.base_media_month_id = r.forecast_media_month_id;
