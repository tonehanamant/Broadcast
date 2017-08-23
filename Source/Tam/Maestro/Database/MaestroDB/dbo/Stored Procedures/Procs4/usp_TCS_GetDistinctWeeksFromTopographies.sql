CREATE PROCEDURE [dbo].[usp_TCS_GetDistinctWeeksFromTopographies]
	@traffic_id int
AS
BEGIN
	select distinct 
		traffic_detail_weeks.traffic_detail_id, 
		traffic_detail_weeks.start_date, 
		traffic_detail_weeks.end_date, 
		traffic_details.network_id, 
		traffic_details.daypart_id, 
		traffic.ratings_daypart_id, 
		traffic_details.spot_length_id, 
		traffic_detail_topographies.daypart_id,
		traffic_detail_weeks.id
	from 
		traffic_detail_topographies(NOLOCK) 
		JOIN traffic_detail_weeks (NOLOCK) on traffic_detail_weeks.id = traffic_detail_topographies.traffic_detail_week_id
		JOIN traffic_details (NOLOCK) on traffic_details.id = traffic_detail_weeks.traffic_detail_id
		JOIN traffic (NOLOCK) on  traffic.id = traffic_details.traffic_id
	where 
		traffic.id = @traffic_id
END
