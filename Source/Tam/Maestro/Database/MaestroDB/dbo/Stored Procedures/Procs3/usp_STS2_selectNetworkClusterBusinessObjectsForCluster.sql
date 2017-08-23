-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_selectNetworkClusterBusinessObjectsForCluster]
	@cluster_id INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT
		network_clusters.cluster_id,
		network_clusters.network_id,
		networks.code
	FROM
		network_clusters
		JOIN networks ON networks.id=network_clusters.network_id
	WHERE
		cluster_id=@cluster_id
END
