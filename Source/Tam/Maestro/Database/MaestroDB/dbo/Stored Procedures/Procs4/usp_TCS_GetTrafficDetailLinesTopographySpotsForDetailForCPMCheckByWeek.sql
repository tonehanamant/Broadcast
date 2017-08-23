CREATE PROCEDURE [dbo].[usp_TCS_GetTrafficDetailLinesTopographySpotsForDetailForCPMCheckByWeek]
	@traffic_detail_id as int,
	@daypart_id as int,
	@start_date as datetime	
AS
BEGIN
	select 
		distinct traffic_details.network_id, 
		topographies.id, 
		topographies.code, 
		sum(traffic_detail_topographies.spots), 
		traffic_detail_topographies.universe, 
		traffic_detail_topographies.rate, 
		traffic_detail_topographies.daypart_id,
		cast(topography_maps.map_value as int)
	from 
		traffic_details (NOLOCK) 
		join traffic_detail_weeks on traffic_detail_weeks.traffic_detail_id = traffic_details.id
		join traffic_detail_topographies (NOLOCK) 
			on traffic_detail_weeks.id = traffic_detail_topographies.traffic_detail_week_id
		join topographies (NOLOCK) 
			on topographies.id = traffic_detail_topographies.topography_id 
		join topography_maps (NOLOCK) 
			on topography_maps.topography_id = topographies.id and topography_maps.map_set = 'traffic'
	where
		traffic_details.id = @traffic_detail_id
		and traffic_detail_weeks.start_date = @start_date
		and traffic_detail_topographies.daypart_id = @daypart_id 
		and traffic_detail_weeks.suspended = 0
	GROUP BY 
		traffic_details.network_id, 
		topographies.id,  
		topographies.code, 
		traffic_detail_topographies.universe, 
		traffic_detail_topographies.rate,
		traffic_detail_topographies.daypart_id,
		cast(topography_maps.map_value as int)  
	order by 
		cast(topography_maps.map_value as int)
END
