CREATE PROCEDURE usp_zone_zones_delete
(
	@primary_zone_id		Int,
	@secondary_zone_id		Int,
	@type		VarChar(15)
)
AS
DELETE FROM zone_zones WHERE primary_zone_id=@primary_zone_id AND secondary_zone_id=@secondary_zone_id AND type=@type
