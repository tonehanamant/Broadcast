CREATE PROCEDURE usp_zone_zones_select_all
AS
SELECT
	*
FROM
	zone_zones WITH(NOLOCK)
