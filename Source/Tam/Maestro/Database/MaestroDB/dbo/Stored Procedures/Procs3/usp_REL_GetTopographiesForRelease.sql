CREATE PROCEDURE [dbo].[usp_REL_GetTopographiesForRelease]
	@release_id int
AS

select distinct
	traffic_detail_topographies.topography_id,
	topographies.code,
	topographies.name,
	topographies.topography_type
from 
	traffic_detail_topographies (NOLOCK) 
	join topographies (NOLOCK) on topographies.id = traffic_detail_topographies.topography_id
	join traffic_detail_weeks (NOLOCK) on traffic_detail_weeks.id = traffic_detail_topographies.traffic_detail_week_id
	join traffic_details (NOLOCK) on traffic_detail_weeks.traffic_detail_id = traffic_details.id  
	join traffic (NOLOCK) on traffic.id = traffic_details.traffic_id
where
	traffic.release_id = @release_id
order by
	topographies.code
