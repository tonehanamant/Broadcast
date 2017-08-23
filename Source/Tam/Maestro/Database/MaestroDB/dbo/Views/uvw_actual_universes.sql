CREATE view [dbo].[uvw_actual_universes] as 
	select	
		u.rating_category_id,
		u.base_media_month_id 'media_month_id',
		u.nielsen_network_id,
		u.audience_id,
		u.universe
	from
		universes u
	where
		u.base_media_month_id = u.forecast_media_month_id;
