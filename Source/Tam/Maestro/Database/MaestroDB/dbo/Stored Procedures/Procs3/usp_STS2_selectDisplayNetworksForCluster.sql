-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- Changed on:	6/6/14
-- Changed By:	Brenton L Reeder
-- Changes:		Added language_id and affilaited_network_id to output
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_selectDisplayNetworksForCluster]
	@cluster_id int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT	
		id,
		code,
		name,
		active,
		flag,
		effective_date,
		language_id,
		affiliated_network_id,
		network_type_id
	FROM	
		networks (NOLOCK)
	WHERE 
		id IN (
			SELECT network_id FROM network_clusters nc (NOLOCK) WHERE 
				nc.cluster_id=@cluster_id 
		)
	ORDER BY 
		name
END
