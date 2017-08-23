CREATE PROCEDURE usp_zone_state_histories_update
(
	@zone_id		Int,
	@state_id		Int,
	@start_date		DateTime,
	@weight		Float,
	@end_date		DateTime
)
AS
UPDATE zone_state_histories SET
	weight = @weight,
	end_date = @end_date
WHERE
	zone_id = @zone_id AND
	state_id = @state_id AND
	start_date = @start_date
