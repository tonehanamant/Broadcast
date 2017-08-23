CREATE PROCEDURE usp_system_group_systems_update
(
	@system_group_id		Int,
	@system_id		Int,
	@effective_date		DateTime
)
AS
UPDATE system_group_systems SET
	effective_date = @effective_date
WHERE
	system_group_id = @system_group_id AND
	system_id = @system_id
