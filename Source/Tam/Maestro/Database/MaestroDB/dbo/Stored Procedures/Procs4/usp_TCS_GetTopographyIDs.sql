
CREATE PROCEDURE [dbo].[usp_TCS_GetTopographyIDs]
( @tid as int)

AS

SELECT 
	distinct 
		traffic_detail_topographies.topography_id 
from 
	traffic_detail_topographies (NOLOCK)
	join traffic_detail_weeks (NOLOCK) on traffic_detail_weeks.id = traffic_detail_topographies.traffic_detail_week_id
	join traffic_details (NOLOCK) on traffic_details.id = traffic_detail_weeks.traffic_detail_id
WHERE 
	traffic_details.traffic_id = @tid 
ORDER BY 
	traffic_detail_topographies.topography_id;

