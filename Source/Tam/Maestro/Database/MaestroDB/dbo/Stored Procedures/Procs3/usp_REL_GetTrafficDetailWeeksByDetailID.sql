CREATE Procedure [dbo].[usp_REL_GetTrafficDetailWeeksByDetailID]
      (
            @traffic_detail_id int
      )

AS

select 
	tdw.id,
	tdw.traffic_detail_id,
	tdw.start_date,
	tdw.end_date,
	tdw.suspended
from
	traffic_detail_weeks (NOLOCK) tdw 
where 
	tdw.traffic_detail_id = @traffic_detail_id
order by 
	tdw.start_date
