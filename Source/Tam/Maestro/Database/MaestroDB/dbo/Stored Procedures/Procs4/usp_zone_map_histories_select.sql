CREATE PROCEDURE usp_zone_map_histories_select
(
	@zone_map_id		Int,
	@start_date		DateTime
)
AS
SELECT
	*
FROM
	zone_map_histories WITH(NOLOCK)
WHERE
	zone_map_id=@zone_map_id
	AND
	start_date=@start_date

