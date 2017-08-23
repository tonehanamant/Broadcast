CREATE PROCEDURE usp_network_clusters_delete
(
	@cluster_id		Int,
	@network_id		Int)
AS
DELETE FROM
	network_clusters
WHERE
	cluster_id = @cluster_id
 AND
	network_id = @network_id
