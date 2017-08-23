CREATE PROCEDURE usp_system_custom_traffic_histories_update
(
	@system_id		Int,
	@start_date		DateTime,
	@traffic_factor		Float,
	@end_date		DateTime
)
AS
UPDATE system_custom_traffic_histories SET
	traffic_factor = @traffic_factor,
	end_date = @end_date
WHERE
	system_id = @system_id AND
	start_date = @start_date
