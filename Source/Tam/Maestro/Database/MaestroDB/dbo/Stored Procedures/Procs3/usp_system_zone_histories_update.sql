CREATE PROCEDURE usp_system_zone_histories_update
(
	@zone_id		Int,
	@system_id		Int,
	@start_date		DateTime,
	@type		VarChar(15),
	@end_date		DateTime
)
AS
UPDATE system_zone_histories SET
	end_date = @end_date
WHERE
	zone_id = @zone_id AND
	system_id = @system_id AND
	start_date = @start_date AND
	type = @type
