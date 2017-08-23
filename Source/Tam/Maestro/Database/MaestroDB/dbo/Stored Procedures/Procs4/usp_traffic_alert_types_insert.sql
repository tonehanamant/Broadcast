CREATE PROCEDURE usp_traffic_alert_types_insert
(
	@id		int		OUTPUT,
	@traffic_alert_type		VarChar(63)
)
AS
INSERT INTO traffic_alert_types
(
	traffic_alert_type
)
VALUES
(
	@traffic_alert_type
)

SELECT
	@id = SCOPE_IDENTITY()
