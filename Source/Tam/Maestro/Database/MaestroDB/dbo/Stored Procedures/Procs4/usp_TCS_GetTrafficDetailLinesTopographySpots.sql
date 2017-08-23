CREATE PROCEDURE [dbo].[usp_TCS_GetTrafficDetailLinesTopographySpots]
	@traffic_id as int
AS
BEGIN
	SELECT DISTINCT
		traffic_details.network_id, 
		traffic_detail_topographies.topography_id, 
		topographies.code, 
		traffic_detail_topographies.spots, 
		traffic_detail_topographies.universe, 
		traffic_detail_weeks.start_date, 
		traffic_detail_weeks.end_date, 
		traffic_detail_topographies.rate, 
		traffic_detail_topographies.daypart_id,
		isnull (cast(topography_maps.map_value as int) , 99),
		isnull (cast(ads_map.map_value as bit), 0),
		isnull (cast(index_map.map_value as bit), 0),
		isnull (cast(spot_precision_map.map_value as int) , 0),
		rate_type_map.map_value,
		isnull (cast(edit_map.map_value as bit) , 0),
		isnull (cast(opto_map.map_value as bit) , 0),
		isnull (release_breakdown_map.map_value , 'dft'),
		traffic_details.id
	FROM 
		traffic_details (NOLOCK)
		join traffic_detail_weeks (NOLOCK) on traffic_details.id = traffic_detail_weeks.traffic_detail_id
		join traffic_detail_topographies (NOLOCK) on traffic_detail_topographies.traffic_detail_week_id = traffic_detail_weeks.id
		join networks (NOLOCK) on networks.id = traffic_details.network_id
		join topographies (NOLOCK) on topographies.id = traffic_detail_topographies.topography_id 
		left join topography_maps (NOLOCK) on topography_maps.topography_id = topographies.id and topography_maps.map_set = 'traffic'
		left join topography_maps ads_map(NOLOCK) on ads_map.topography_id = topographies.id and ads_map.map_set = 'traffic_ads'
		left join topography_maps index_map(NOLOCK) on index_map.topography_id = topographies.id and index_map.map_set = 'traffic_index'
		left join topography_maps spot_precision_map(NOLOCK) on spot_precision_map.topography_id = topographies.id and spot_precision_map.map_set = 'spot_precision'
		left join topography_maps rate_type_map(NOLOCK) on rate_type_map.topography_id = topographies.id and rate_type_map.map_set = 'rate_type'
		left join topography_maps edit_map(NOLOCK) on edit_map.topography_id = topographies.id and edit_map.map_set = 'topo_edit'
		left join topography_maps opto_map(NOLOCK) on opto_map.topography_id = topographies.id and opto_map.map_set = 'topo_opto'
		left join topography_maps release_breakdown_map(NOLOCK) on release_breakdown_map.topography_id = topographies.id and release_breakdown_map.map_set = 'release_bd_model'
	WHERE
		traffic_details.traffic_id = @traffic_id
	ORDER BY 
		traffic_details.id, 
		traffic_detail_weeks.start_date,
		isnull (cast(topography_maps.map_value as int) , 99) ,  
		traffic_detail_topographies.topography_id
END
