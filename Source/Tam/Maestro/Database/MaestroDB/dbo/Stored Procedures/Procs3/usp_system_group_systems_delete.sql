CREATE PROCEDURE usp_system_group_systems_delete
(
	@system_group_id		Int,
	@system_id		Int
)
AS
DELETE FROM system_group_systems WHERE system_group_id=@system_group_id AND system_id=@system_id
