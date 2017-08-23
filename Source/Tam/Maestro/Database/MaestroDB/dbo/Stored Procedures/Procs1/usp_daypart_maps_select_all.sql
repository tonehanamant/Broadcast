CREATE PROCEDURE usp_daypart_maps_select_all
AS
SELECT
	*
FROM
	daypart_maps WITH(NOLOCK)
