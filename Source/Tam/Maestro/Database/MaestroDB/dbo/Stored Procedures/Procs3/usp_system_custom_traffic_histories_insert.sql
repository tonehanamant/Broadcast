CREATE PROCEDURE usp_system_custom_traffic_histories_insert
(
	@system_id		Int,
	@start_date		DateTime,
	@traffic_factor		Float,
	@end_date		DateTime
)
AS
INSERT INTO system_custom_traffic_histories
(
	system_id,
	start_date,
	traffic_factor,
	end_date
)
VALUES
(
	@system_id,
	@start_date,
	@traffic_factor,
	@end_date
)

