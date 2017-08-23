CREATE PROCEDURE usp_daypart_days_delete
(
	@daypart_id		Int,
	@day_id		Int)
AS
DELETE FROM
	daypart_days
WHERE
	daypart_id = @daypart_id
 AND
	day_id = @day_id
