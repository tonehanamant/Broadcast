CREATE PROCEDURE usp_traffic_alert_types_select_all
AS
SELECT
	id,
	traffic_alert_type
FROM
	traffic_alert_types (NOLOCK)
