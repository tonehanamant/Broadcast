CREATE PROCEDURE [dbo].[usp_STS2_selectDisplayClusters]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT
		clusters.id,
		clusters.name,
		clusters.topography_id,
		COUNT(*)
	FROM
		network_clusters (NOLOCK)
		JOIN clusters (NOLOCK) ON clusters.id=network_clusters.cluster_id
	GROUP BY
		clusters.id,
		clusters.name,
		clusters.topography_id
	ORDER BY
		clusters.name
END

