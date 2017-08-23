
CREATE PROCEDURE [dbo].[usp_REL_GetAvailableTrafficAlerts]
AS
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;
SELECT 
	t.id,
	t.alert_comment,
	t.traffic_id,
	t.traffic_alert_type_id,
	t.copy_comment,
	tat.traffic_alert_type,
	cancellation_id = 0
FROM
	traffic_alerts t
	JOIN traffic_alert_types tat ON t.traffic_alert_type_id = tat.id
WHERE
	t.id NOT IN (SELECT traffic_alert_id FROM traffic_master_alert_traffic_alerts)
ORDER BY
	t.id

