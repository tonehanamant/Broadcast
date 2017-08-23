CREATE PROCEDURE usp_system_zones_update
(
	@zone_id		Int,
	@system_id		Int,
	@type		VarChar(15),
	@effective_date		DateTime
)
AS
UPDATE system_zones SET
	effective_date = @effective_date
WHERE
	zone_id = @zone_id AND
	system_id = @system_id AND
	type = @type
