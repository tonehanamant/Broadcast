CREATE PROCEDURE usp_system_daypart_histories_update
(
	@system_id		Int,
	@daypart_id		Int,
	@start_date		DateTime,
	@end_date		DateTime
)
AS
UPDATE system_daypart_histories SET
	end_date = @end_date
WHERE
	system_id = @system_id AND
	daypart_id = @daypart_id AND
	start_date = @start_date
