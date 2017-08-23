CREATE Procedure [dbo].[usp_REL_GetTrafficMasterAlertMapForTrafficAlert]
      (
            @traffic_alert_id int
      )
AS

select 
	traffic_master_alert_traffic_alerts.id,
	traffic_master_alert_traffic_alerts.traffic_master_alert_id,
	traffic_master_alert_traffic_alerts.traffic_alert_id,
	traffic_master_alert_traffic_alerts.rank
from
	traffic_master_alert_traffic_alerts (NOLOCK) 
where
	traffic_master_alert_traffic_alerts.traffic_alert_id = @traffic_alert_id
