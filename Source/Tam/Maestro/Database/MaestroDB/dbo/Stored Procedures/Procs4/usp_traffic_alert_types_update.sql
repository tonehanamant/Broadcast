CREATE PROCEDURE usp_traffic_alert_types_update
(
	@id		Int,
	@traffic_alert_type		VarChar(63)
)
AS
UPDATE traffic_alert_types SET
	traffic_alert_type = @traffic_alert_type
WHERE
	id = @id
