
CREATE PROCEDURE [dbo].[usp_TCS_GetTopographyInfo]
( @tid as int)

AS
SELECT 
	distinct 
	topographies.code,
	traffic_detail_topographies.topography_id,
	cast (topography_maps.map_value as int),
	isnull (cast(edit_map.map_value as bit) , 0),
	isnull (cast(opto_map.map_value as bit) , 0),
	isnull (release_breakdown_map.map_value , 'dft'),
	isnull (rate_type_map.map_value , 'var')
from 
	traffic_detail_topographies (NOLOCK)
	join traffic_detail_weeks (NOLOCK) on traffic_detail_topographies.traffic_detail_week_id = traffic_detail_weeks.id
	join traffic_details (NOLOCK) on traffic_details.id = traffic_detail_weeks.traffic_detail_id
	join topographies (NOLOCK) on traffic_detail_topographies.topography_id = topographies.id 
	left join topography_maps (NOLOCK) on topography_maps.topography_id = topographies.id and topography_maps.map_set = 'traffic'
	left join topography_maps edit_map(NOLOCK) on edit_map.topography_id = topographies.id and edit_map.map_set = 'topo_edit'
	left join topography_maps opto_map(NOLOCK) on opto_map.topography_id = topographies.id and opto_map.map_set = 'topo_opto'
	left join topography_maps release_breakdown_map(NOLOCK) on release_breakdown_map.topography_id = topographies.id and release_breakdown_map.map_set = 'release_bd_model'
	left join topography_maps rate_type_map(NOLOCK) on rate_type_map.topography_id = topographies.id and rate_type_map.map_set = 'rate_type'

WHERE 
	traffic_details.traffic_id = @tid
ORDER BY 
	cast (topography_maps.map_value as int);
