CREATE PROCEDURE usp_system_daypart_histories_insert
(
	@system_id		Int,
	@daypart_id		Int,
	@start_date		DateTime,
	@end_date		DateTime
)
AS
INSERT INTO system_daypart_histories
(
	system_id,
	daypart_id,
	start_date,
	end_date
)
VALUES
(
	@system_id,
	@daypart_id,
	@start_date,
	@end_date
)

