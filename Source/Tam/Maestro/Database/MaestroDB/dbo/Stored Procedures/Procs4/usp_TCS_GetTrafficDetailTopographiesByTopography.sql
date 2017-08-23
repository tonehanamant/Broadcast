CREATE PROCEDURE [dbo].[usp_TCS_GetTrafficDetailTopographiesByTopography]
	@topography_id Int,
	@traffic_id Int
AS
BEGIN
	select 
		traffic_detail_topographies.topography_id, 
		traffic_detail_topographies.spots, 
		traffic_detail_topographies.universe, 
		traffic_detail_topographies.rate,
		traffic_detail_topographies.daypart_id,
		traffic_detail_topographies.traffic_detail_week_id
	from 
		traffic_detail_topographies (NOLOCK) 
		join traffic_detail_weeks (NOLOCK) on traffic_detail_weeks.id = traffic_detail_topographies.traffic_detail_week_id
		join traffic_details  (NOLOCK) on traffic_detail_weeks.traffic_detail_id = traffic_details.id
	where 
		traffic_details.traffic_id = @traffic_id 
		and traffic_detail_topographies.topography_id = @topography_id 	
END
