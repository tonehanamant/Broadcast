CREATE PROCEDURE [dbo].[usp_TCS_GetTrafficDetailLinesTopographySpotsForDetailForCPMCheck]
	@traffic_detail_id as int
AS
BEGIN
	SELECT DISTINCT
		traffic_details.network_id, 
		topographies.id,  
		topographies.code, 
		sum(traffic_detail_topographies.spots), 
		traffic_detail_topographies.universe, 
		traffic_detail_topographies.rate, 
		traffic_detail_topographies.daypart_id  
	FROM 
		traffic_details (NOLOCK)  
		join traffic_detail_weeks (NOLOCK) on traffic_details.id = traffic_detail_weeks.traffic_detail_id
		join traffic_detail_topographies (NOLOCK) on traffic_detail_topographies.traffic_detail_week_id = traffic_detail_weeks.id 
		join topographies (NOLOCK) on topographies.id = traffic_detail_topographies.topography_id
	WHERE
		traffic_details.id = @traffic_detail_id
	GROUP BY 
		traffic_details.network_id, 
		topographies.id,  
		topographies.code, 
		traffic_detail_topographies.universe, 
		traffic_detail_topographies.rate,
		traffic_detail_topographies.daypart_id  
	ORDER BY 
		topographies.id
END
