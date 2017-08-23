CREATE PROCEDURE [dbo].[usp_REL_GetTrafficAlertIdsForMasterAlert]
(
	@traffic_master_alert_id Int
)
AS

select 
	tma.id, 
	tma.traffic_master_alert_id,		
	tma.traffic_alert_id,
	tma.rank
from
	traffic_master_alert_traffic_alerts (NOLOCK) tma
where 
	tma.traffic_master_alert_id = @traffic_master_alert_id
order by 
	tma.rank
