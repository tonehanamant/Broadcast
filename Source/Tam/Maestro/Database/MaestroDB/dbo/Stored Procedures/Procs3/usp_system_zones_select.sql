CREATE PROCEDURE usp_system_zones_select
(
	@zone_id		Int,
	@system_id		Int,
	@type		VarChar(15)
)
AS
SELECT
	*
FROM
	system_zones WITH(NOLOCK)
WHERE
	zone_id=@zone_id
	AND
	system_id=@system_id
	AND
	type=@type

