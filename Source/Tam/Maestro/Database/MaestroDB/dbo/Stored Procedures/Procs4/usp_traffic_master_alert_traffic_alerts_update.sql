CREATE PROCEDURE [dbo].[usp_traffic_master_alert_traffic_alerts_update]
(
	@id		Int,
	@traffic_master_alert_id		Int,
	@traffic_alert_id		Int,
	@rank		Int
)
AS
UPDATE traffic_master_alert_traffic_alerts SET
	traffic_master_alert_id = @traffic_master_alert_id,
	traffic_alert_id = @traffic_alert_id,
	rank = @rank
WHERE
	id = @id
grant execute on usp_traffic_master_alert_traffic_alerts_update to Production
