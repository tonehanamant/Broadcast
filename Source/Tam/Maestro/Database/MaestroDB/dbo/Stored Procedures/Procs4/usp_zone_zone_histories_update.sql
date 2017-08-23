CREATE PROCEDURE usp_zone_zone_histories_update
(
	@primary_zone_id		Int,
	@secondary_zone_id		Int,
	@type		VarChar(15),
	@start_date		DateTime,
	@end_date		DateTime
)
AS
UPDATE zone_zone_histories SET
	end_date = @end_date
WHERE
	primary_zone_id = @primary_zone_id AND
	secondary_zone_id = @secondary_zone_id AND
	type = @type AND
	start_date = @start_date
