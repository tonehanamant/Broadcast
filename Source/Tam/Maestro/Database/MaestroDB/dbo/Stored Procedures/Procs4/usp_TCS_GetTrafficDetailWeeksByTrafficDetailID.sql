
CREATE PROCEDURE [dbo].[usp_TCS_GetTrafficDetailWeeksByTrafficDetailID]
(
      @traffic_detail_id int
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
where 
	traffic_detail_weeks.traffic_detail_id = @traffic_detail_id
