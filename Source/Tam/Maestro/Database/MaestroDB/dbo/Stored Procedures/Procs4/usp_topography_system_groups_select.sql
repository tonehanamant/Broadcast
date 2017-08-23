CREATE PROCEDURE usp_topography_system_groups_select
(
	@topography_id		Int,
	@system_group_id		Int
)
AS
SELECT
	*
FROM
	topography_system_groups WITH(NOLOCK)
WHERE
	topography_id=@topography_id
	AND
	system_group_id=@system_group_id

