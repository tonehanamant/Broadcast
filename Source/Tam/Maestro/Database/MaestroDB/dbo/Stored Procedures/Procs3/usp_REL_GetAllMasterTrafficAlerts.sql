CREATE Procedure [dbo].[usp_REL_GetAllMasterTrafficAlerts]

AS

select 
	tma.id, 
	tma.name, 
	count(tmata.traffic_alert_id)
from 
	traffic_master_alerts (NOLOCK) tma
	left join traffic_master_alert_traffic_alerts (NOLOCK) tmata on tmata.traffic_master_alert_id = tma.id
group by
	tma.id, 
	tma.name
order by
	tma.id
