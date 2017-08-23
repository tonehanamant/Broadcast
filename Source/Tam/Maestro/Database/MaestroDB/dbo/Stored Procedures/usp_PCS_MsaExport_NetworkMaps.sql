-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 6/15/2015
-- Description:	Used to populate MSA Export file "network_maps.txt"
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_MsaExport_NetworkMaps]
	@msa_tam_post_proposal_ids UniqueIdTable READONLY
AS
BEGIN
	SET NOCOUNT ON;
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

    SELECT
		nm.id 'id', 
		nm.map_set 'map_set', 
		nm.network_id 'net_id', 
		nm.map_value 'map_value' 
	FROM 
		network_maps nm 
	WHERE
		nm.active=1 
		AND nm.map_set IN ('excel', 'Nielsen') 
	GROUP BY 
		nm.id, nm.map_set, nm.network_id, nm.map_value 
	ORDER BY 
		nm.map_set, nm.map_value
END