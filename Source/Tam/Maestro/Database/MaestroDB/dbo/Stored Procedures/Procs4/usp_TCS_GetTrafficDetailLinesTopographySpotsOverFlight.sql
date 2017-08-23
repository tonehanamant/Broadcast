CREATE PROCEDURE [dbo].[usp_TCS_GetTrafficDetailLinesTopographySpotsOverFlight]
    @traffic_id as int
AS
BEGIN
	select distinct 
		traffic_details.network_id, 
		traffic_detail_topographies.topography_id, 
		topographies.code, 
		avg(traffic_detail_topographies.spots), 
		traffic_detail_topographies.universe, 
		min(traffic_detail_weeks.start_date), 
		max(traffic_detail_weeks.end_date), 
		traffic_detail_topographies.rate, 
		traffic_details.daypart_id,
		isnull (cast(topography_maps.map_value as int) , 99),
		ads_map.map_value,
		index_map.map_value,
		isnull (cast(spot_precision_map.map_value as int) , 0),
		rate_type_map.map_value,
		traffic_details.id
	from 
		traffic_details (NOLOCK)
		join traffic_detail_weeks (NOLOCK) on traffic_details.id = traffic_detail_weeks.traffic_detail_id
		join traffic_detail_topographies (NOLOCK) on traffic_detail_weeks.id = traffic_detail_topographies.traffic_detail_week_id
		join topographies (NOLOCK) on topographies.id = traffic_detail_topographies.topography_id 
		left join topography_maps (NOLOCK) on topography_maps.topography_id = topographies.id and topography_maps.map_set = 'traffic'
		left join topography_maps ads_map(NOLOCK) on ads_map.topography_id = topographies.id and ads_map.map_set = 'traffic_ads'
		left join topography_maps index_map(NOLOCK) on index_map.topography_id = topographies.id and index_map.map_set = 'traffic_index'
		left join topography_maps spot_precision_map(NOLOCK) on spot_precision_map.topography_id = topographies.id and spot_precision_map.map_set = 'spot_precision'
		left join topography_maps rate_type_map(NOLOCK) on rate_type_map.topography_id = topographies.id and rate_type_map.map_set = 'rate_type'
	where
		traffic_details.traffic_id = @traffic_id 
	GROUP BY
		traffic_details.network_id, 
		traffic_detail_topographies.topography_id, 
		topographies.code,
		traffic_detail_topographies.universe,
		traffic_detail_topographies.rate, 
		traffic_details.daypart_id,
		isnull (cast(topography_maps.map_value as int) , 99),
		ads_map.map_value,
		index_map.map_value,
		isnull (cast(spot_precision_map.map_value as int) , 0),
		rate_type_map.map_value,
		traffic_details.id
	ORDER BY 
		traffic_details.id,
		isnull (cast(topography_maps.map_value as int) , 99) ,  
		traffic_detail_topographies.topography_id
END
