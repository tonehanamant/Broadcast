CREATE PROCEDURE usp_topography_system_groups_delete
(
	@topography_id		Int,
	@system_group_id		Int
)
AS
DELETE FROM topography_system_groups WHERE topography_id=@topography_id AND system_group_id=@system_group_id
