
CREATE PROCEDURE [dbo].[usp_STS2_selectDisplayCluster]
	@cluster_id INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT
		clusters.id,
		clusters.name,
		COUNT(*),
		clusters.topography_id,
		clusters.cluster_type
	FROM
		network_clusters (NOLOCK)
		JOIN clusters (NOLOCK) ON clusters.id=network_clusters.cluster_id
	WHERE
		clusters.id=@cluster_id
	GROUP BY
		clusters.id,
		clusters.name,
		clusters.topography_id,
		clusters.cluster_type
	ORDER BY
		clusters.name
END

