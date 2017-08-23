CREATE PROCEDURE usp_system_custom_traffic_update
(
	@system_id		Int,
	@traffic_factor		Float,
	@effective_date		DateTime
)
AS
UPDATE system_custom_traffic SET
	traffic_factor = @traffic_factor,
	effective_date = @effective_date
WHERE
	system_id = @system_id
