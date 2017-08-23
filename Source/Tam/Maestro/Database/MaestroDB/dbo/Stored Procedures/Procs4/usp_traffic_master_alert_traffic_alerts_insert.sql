CREATE PROCEDURE [dbo].[usp_traffic_master_alert_traffic_alerts_insert]
(
	@id		int		OUTPUT,
	@traffic_master_alert_id		Int,
	@traffic_alert_id		Int,
	@rank		Int
)
AS
INSERT INTO traffic_master_alert_traffic_alerts
(
	traffic_master_alert_id,
	traffic_alert_id,
	rank
)
VALUES
(
	@traffic_master_alert_id,
	@traffic_alert_id,
	@rank
)

SELECT
	@id = SCOPE_IDENTITY()
