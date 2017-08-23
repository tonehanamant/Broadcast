CREATE PROCEDURE [wb].[usp_WB_GetScheduleDistinctDates]
as
select 
      distinct mw.start_date
from 
      wb.wb_schedules (NOLOCK) wbs
      join dbo.media_weeks (NOLOCK) mw on wbs.media_week_id = mw.id 
order by
      mw.start_date DESC