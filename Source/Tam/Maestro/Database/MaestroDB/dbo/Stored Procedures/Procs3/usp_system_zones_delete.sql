CREATE PROCEDURE usp_system_zones_delete
(
	@zone_id		Int,
	@system_id		Int,
	@type		VarChar(15)
)
AS
DELETE FROM system_zones WHERE zone_id=@zone_id AND system_id=@system_id AND type=@type
