CREATE PROCEDURE [dbo].[usp_PCS_GetClusterNetworks]
AS
BEGIN
	SELECT id,name FROM clusters (NOLOCK) 
	SELECT cluster_id,network_id FROM network_clusters (NOLOCK) 
END
