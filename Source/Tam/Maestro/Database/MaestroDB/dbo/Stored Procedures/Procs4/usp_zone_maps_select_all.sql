CREATE PROCEDURE usp_zone_maps_select_all
AS
SELECT
	*
FROM
	zone_maps WITH(NOLOCK)
