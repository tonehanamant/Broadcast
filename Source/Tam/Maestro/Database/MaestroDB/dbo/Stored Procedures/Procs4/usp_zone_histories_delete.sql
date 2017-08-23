CREATE PROCEDURE usp_zone_histories_delete
(
	@zone_id		Int,
	@start_date		DateTime)
AS
BEGIN
DELETE FROM
	zone_histories
WHERE
	zone_id = @zone_id
 AND
	start_date = @start_date
END
