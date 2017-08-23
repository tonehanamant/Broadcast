CREATE PROCEDURE usp_zone_zone_histories_delete
(
	@primary_zone_id		Int,
	@secondary_zone_id		Int,
	@type		VarChar(15),
	@start_date		DateTime)
AS
DELETE FROM
	zone_zone_histories
WHERE
	primary_zone_id = @primary_zone_id
 AND
	secondary_zone_id = @secondary_zone_id
 AND
	type = @type
 AND
	start_date = @start_date
