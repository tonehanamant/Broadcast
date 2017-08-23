CREATE PROCEDURE usp_system_daypart_histories_delete
(
	@system_id		Int,
	@daypart_id		Int,
	@start_date		DateTime)
AS
DELETE FROM
	system_daypart_histories
WHERE
	system_id = @system_id
 AND
	daypart_id = @daypart_id
 AND
	start_date = @start_date
