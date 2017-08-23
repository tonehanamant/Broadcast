CREATE PROCEDURE usp_system_custom_traffic_insert
(
	@system_id		Int,
	@traffic_factor		Float,
	@effective_date		DateTime
)
AS
INSERT INTO system_custom_traffic
(
	system_id,
	traffic_factor,
	effective_date
)
VALUES
(
	@system_id,
	@traffic_factor,
	@effective_date
)

