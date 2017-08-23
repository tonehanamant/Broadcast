CREATE PROCEDURE usp_zone_map_histories_delete
(
	@zone_map_id		Int,
	@start_date		DateTime)
AS
DELETE FROM
	zone_map_histories
WHERE
	zone_map_id = @zone_map_id
 AND
	start_date = @start_date
