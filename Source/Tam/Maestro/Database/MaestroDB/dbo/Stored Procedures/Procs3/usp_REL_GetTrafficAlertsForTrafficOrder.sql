
CREATE Procedure [dbo].[usp_REL_GetTrafficAlertsForTrafficOrder]
	  (
			@traffic_id int
	  )
AS
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED; --SAME AS NO LOCK
select 
	ta.id,
	ta.alert_comment,
	ta.traffic_id,
	ta.traffic_alert_type_id,
	tat.traffic_alert_type,
	ta.copy_comment,
	master_alert_id = tma.id,
	master_alert_name = tma.name,
	cancellation_id = 0
from
	traffic_alerts ta (NOLOCK) 
	join traffic_alert_types tat (NOLOCK) on ta.traffic_alert_type_id = tat.id
	left join traffic_master_alert_traffic_alerts tmata on ta.id = tmata.traffic_alert_id
	left join traffic_master_alerts tma on tmata.traffic_master_alert_id = tma.id
where
	ta.traffic_id = @traffic_id
order by
	ta.id
