CREATE PROCEDURE usp_topography_systems_select
(
	@topography_id		Int,
	@system_id		Int
)
AS
SELECT
	*
FROM
	topography_systems WITH(NOLOCK)
WHERE
	topography_id=@topography_id
	AND
	system_id=@system_id

