CREATE PROCEDURE usp_network_clusters_select_all
AS
SELECT
	*
FROM
	network_clusters WITH(NOLOCK)
