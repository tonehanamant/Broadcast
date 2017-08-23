CREATE PROCEDURE usp_timespans_update
(
	@id		Int,
	@start_time		Int,
	@end_time		Int
)
AS
UPDATE timespans SET
	start_time = @start_time,
	end_time = @end_time
WHERE
	id = @id

