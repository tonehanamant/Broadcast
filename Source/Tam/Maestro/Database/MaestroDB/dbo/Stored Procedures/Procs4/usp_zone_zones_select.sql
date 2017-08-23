CREATE PROCEDURE usp_zone_zones_select
(
	@primary_zone_id		Int,
	@secondary_zone_id		Int,
	@type		VarChar(15)
)
AS
SELECT
	*
FROM
	zone_zones WITH(NOLOCK)
WHERE
	primary_zone_id=@primary_zone_id
	AND
	secondary_zone_id=@secondary_zone_id
	AND
	type=@type

