CREATE PROCEDURE usp_topography_zone_histories_select_all
AS
SELECT
	*
FROM
	topography_zone_histories WITH(NOLOCK)
