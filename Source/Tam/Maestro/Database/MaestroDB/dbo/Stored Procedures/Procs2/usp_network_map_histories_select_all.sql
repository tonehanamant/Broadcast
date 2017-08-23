CREATE PROCEDURE usp_network_map_histories_select_all
AS
SELECT
	*
FROM
	network_map_histories WITH(NOLOCK)
