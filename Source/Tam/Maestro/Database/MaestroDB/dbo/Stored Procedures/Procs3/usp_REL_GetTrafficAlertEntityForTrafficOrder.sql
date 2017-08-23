CREATE Procedure [dbo].[usp_REL_GetTrafficAlertEntityForTrafficOrder]
      (
            @traffic_id int
      )
AS

select 
	traffic_alerts.id,
	traffic_alerts.alert_comment,
	traffic_alerts.traffic_id,
	traffic_alerts.traffic_alert_type_id,
	traffic_alerts.copy_comment
from
	traffic_alerts (NOLOCK) 
where
	traffic_alerts.traffic_id = @traffic_id
order by
	traffic_alerts.id
