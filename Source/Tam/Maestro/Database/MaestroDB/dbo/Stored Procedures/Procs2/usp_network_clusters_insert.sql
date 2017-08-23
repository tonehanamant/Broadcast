CREATE PROCEDURE usp_network_clusters_insert
(
	@cluster_id		Int,
	@network_id		Int
)
AS
INSERT INTO network_clusters
(
	cluster_id,
	network_id
)
VALUES
(
	@cluster_id,
	@network_id
)

