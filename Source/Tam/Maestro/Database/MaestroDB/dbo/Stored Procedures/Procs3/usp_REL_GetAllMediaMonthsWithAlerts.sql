CREATE PROCEDURE [dbo].[usp_REL_GetAllMediaMonthsWithAlerts]

AS

select 
	distinct media_months.id, 
	media_months.month, 
	media_months.year,
	media_months.media_month,
	media_months.start_date,
	media_months.end_date
from 
	media_months (NOLOCK)
	join traffic_master_alerts (NOLOCK) on traffic_master_alerts.date_created >= media_months.start_date
	and
	traffic_master_alerts.date_created <= media_months.end_date
WHERE 
		media_months.start_date >= DATEADD(year, -1, GETDATE())
order by
	media_months.id;

