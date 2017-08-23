CREATE PROCEDURE usp_topography_zones_delete
(
	@topography_id		Int,
	@zone_id		Int
)
AS
DELETE FROM topography_zones WHERE topography_id=@topography_id AND zone_id=@zone_id
