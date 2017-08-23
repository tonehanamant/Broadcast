CREATE PROCEDURE usp_zone_zones_update
(
	@primary_zone_id		Int,
	@secondary_zone_id		Int,
	@type		VarChar(15),
	@effective_date		DateTime
)
AS
UPDATE zone_zones SET
	effective_date = @effective_date
WHERE
	primary_zone_id = @primary_zone_id AND
	secondary_zone_id = @secondary_zone_id AND
	type = @type
