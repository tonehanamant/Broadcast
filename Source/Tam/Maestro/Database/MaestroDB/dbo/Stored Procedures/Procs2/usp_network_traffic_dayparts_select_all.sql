CREATE PROCEDURE usp_network_traffic_dayparts_select_all
AS
SELECT
	*
FROM
	network_traffic_dayparts WITH(NOLOCK)
