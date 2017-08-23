


-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_selectNetworkItemsForCluster]
	@cluster_id int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT id,code,effective_date FROM networks (NOLOCK) WHERE 
		id IN 
		(
			SELECT network_id FROM network_clusters nc (NOLOCK) WHERE 
				nc.cluster_id=@cluster_id
		) 
	ORDER BY 
		code
END



