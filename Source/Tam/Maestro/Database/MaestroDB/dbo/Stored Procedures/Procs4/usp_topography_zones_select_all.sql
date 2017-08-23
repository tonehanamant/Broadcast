CREATE PROCEDURE usp_topography_zones_select_all
AS
SELECT
	*
FROM
	topography_zones WITH(NOLOCK)
