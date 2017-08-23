
CREATE Procedure [dbo].[usp_TCS_IsTrafficOrderSuspended]
	(
		@traffic_id Int
	)
AS
select 
	distinct traffic_detail_weeks.start_date 
from 
	traffic_detail_weeks (NOLOCK)
	join traffic_details (NOLOCK) on traffic_details.id = traffic_detail_weeks.traffic_detail_id
where
	traffic_details.traffic_id = @traffic_id 
	and traffic_detail_weeks.suspended = 1
