
CREATE Procedure [dbo].[usp_REL_GetTrafficAlertsForMasterAlert]
	  (
			@traffic_master_alert_id int
	  )
AS

SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;
SET NOCOUNT ON;

select 
	t.id,
	t.alert_comment,
	t.traffic_id,
	t.traffic_alert_type_id,
	tat.traffic_alert_type,
	t.copy_comment,
	tmata.rank,
	cancellation_id = 0
from
	traffic_alerts t
	join traffic_alert_types tat on t.traffic_alert_type_id = tat.id
	join traffic_master_alert_traffic_alerts tmata ON t.id = tmata.traffic_alert_id
WHERE tmata.traffic_master_alert_id = @traffic_master_alert_id
order by
	tmata.rank,
	t.id

