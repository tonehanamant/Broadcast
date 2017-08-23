CREATE PROCEDURE usp_topography_systems_delete
(
	@topography_id		Int,
	@system_id		Int
)
AS
DELETE FROM topography_systems WHERE topography_id=@topography_id AND system_id=@system_id
