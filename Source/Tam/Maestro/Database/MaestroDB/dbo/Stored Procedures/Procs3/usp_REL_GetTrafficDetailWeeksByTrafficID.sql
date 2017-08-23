CREATE Procedure [dbo].[usp_REL_GetTrafficDetailWeeksByTrafficID]
      (
            @traffic_id int
      )

AS

select distinct
	0,
	0,
	tdw.start_date,
	tdw.end_date,
	tdw.suspended
from
	traffic_detail_weeks (NOLOCK) tdw 
	join traffic_details (NOLOCK) td on tdw.traffic_detail_id = td.id
where 
	td.traffic_id = @traffic_id
	and tdw.suspended = (SELECT  TOP 1 WITH ties tdw2.suspended
						 FROM  traffic_detail_weeks (NOLOCK) tdw2
						 WHERE tdw2.start_date = tdw.start_date and 
								tdw2.end_date = tdw.end_date and
								tdw2.traffic_detail_id = td.id
						 GROUP  BY tdw2.suspended
						 ORDER  BY COUNT(*) DESC)
order by 
	tdw.start_date
