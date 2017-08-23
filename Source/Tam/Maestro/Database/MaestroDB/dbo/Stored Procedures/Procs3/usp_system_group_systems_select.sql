CREATE PROCEDURE usp_system_group_systems_select
(
	@system_group_id		Int,
	@system_id		Int
)
AS
SELECT
	*
FROM
	system_group_systems WITH(NOLOCK)
WHERE
	system_group_id=@system_group_id
	AND
	system_id=@system_id

