CREATE PROCEDURE usp_network_clusters_select
(
	@cluster_id		Int,
	@network_id		Int
)
AS
SELECT
	*
FROM
	network_clusters WITH(NOLOCK)
WHERE
	cluster_id=@cluster_id
	AND
	network_id=@network_id

