CREATE PROCEDURE usp_topography_zone_histories_select
(
	@topography_id		Int,
	@zone_id		Int,
	@start_date		DateTime
)
AS
SELECT
	*
FROM
	topography_zone_histories WITH(NOLOCK)
WHERE
	topography_id=@topography_id
	AND
	zone_id=@zone_id
	AND
	start_date=@start_date

