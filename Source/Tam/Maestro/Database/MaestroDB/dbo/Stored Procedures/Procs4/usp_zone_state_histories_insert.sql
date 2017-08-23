CREATE PROCEDURE usp_zone_state_histories_insert
(
	@zone_id		Int,
	@state_id		Int,
	@start_date		DateTime,
	@weight		Float,
	@end_date		DateTime
)
AS
INSERT INTO zone_state_histories
(
	zone_id,
	state_id,
	start_date,
	weight,
	end_date
)
VALUES
(
	@zone_id,
	@state_id,
	@start_date,
	@weight,
	@end_date
)

