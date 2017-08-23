CREATE PROCEDURE usp_audience_maps_select_all
AS
SELECT
	*
FROM
	audience_maps WITH(NOLOCK)
