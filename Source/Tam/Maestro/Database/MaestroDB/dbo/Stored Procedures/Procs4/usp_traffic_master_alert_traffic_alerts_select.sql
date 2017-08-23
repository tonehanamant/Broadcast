CREATE PROCEDURE [dbo].[usp_traffic_master_alert_traffic_alerts_select]
(
	@id Int
)
AS
SELECT
	id,
	traffic_master_alert_id,
	traffic_alert_id,
	rank
FROM
	traffic_master_alert_traffic_alerts (NOLOCK)
WHERE
	id = @id
