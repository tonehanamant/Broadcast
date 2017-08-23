CREATE PROCEDURE usp_network_traffic_daypart_histories_select_all
AS
SELECT
	*
FROM
	network_traffic_daypart_histories WITH(NOLOCK)
