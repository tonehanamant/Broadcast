CREATE PROCEDURE usp_network_break_histories_update
(
	@network_break_id		Int,
	@start_date		DateTime,
	@nielsen_network_id		Int,
	@seconds_after_hour		Int,
	@length		Int,
	@end_date		DateTime,
	@active		Bit
)
AS
UPDATE network_break_histories SET
	nielsen_network_id = @nielsen_network_id,
	seconds_after_hour = @seconds_after_hour,
	length = @length,
	end_date = @end_date,
	active = @active
WHERE
	network_break_id = @network_break_id AND
	start_date = @start_date
