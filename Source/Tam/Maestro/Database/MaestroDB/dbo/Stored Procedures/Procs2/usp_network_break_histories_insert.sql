CREATE PROCEDURE usp_network_break_histories_insert
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
INSERT INTO network_break_histories
(
	network_break_id,
	start_date,
	nielsen_network_id,
	seconds_after_hour,
	length,
	end_date,
	active
)
VALUES
(
	@network_break_id,
	@start_date,
	@nielsen_network_id,
	@seconds_after_hour,
	@length,
	@end_date,
	@active
)

