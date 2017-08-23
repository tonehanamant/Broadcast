CREATE PROCEDURE usp_zone_map_histories_select_all
AS
SELECT
	*
FROM
	zone_map_histories WITH(NOLOCK)
