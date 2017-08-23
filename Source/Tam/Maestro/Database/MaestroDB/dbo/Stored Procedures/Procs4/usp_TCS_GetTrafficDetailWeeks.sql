CREATE PROCEDURE [dbo].[usp_TCS_GetTrafficDetailWeeks]
(
      @traffic_id int
)

AS

select 
	traffic_detail_weeks.id,
	traffic_detail_weeks.traffic_detail_id,
	traffic_detail_weeks.start_date,
	traffic_detail_weeks.end_date,
	traffic_detail_weeks.suspended
from 
	traffic_detail_weeks (NOLOCK)
	join traffic_details on traffic_details.id = traffic_detail_weeks.traffic_detail_id
where 
	traffic_details.traffic_id = @traffic_id
