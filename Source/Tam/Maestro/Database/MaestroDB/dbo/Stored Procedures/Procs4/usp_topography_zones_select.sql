CREATE PROCEDURE usp_topography_zones_select
(
	@topography_id		Int,
	@zone_id		Int
)
AS
SELECT
	*
FROM
	topography_zones WITH(NOLOCK)
WHERE
	topography_id=@topography_id
	AND
	zone_id=@zone_id

