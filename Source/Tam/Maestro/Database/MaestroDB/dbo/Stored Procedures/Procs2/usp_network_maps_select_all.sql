CREATE PROCEDURE usp_network_maps_select_all
AS
SELECT
	*
FROM
	network_maps WITH(NOLOCK)
