-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 9/21/2011
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE usp_BRS_GetNetworksInOrders
AS
BEGIN
	SELECT DISTINCT 
		CASE WHEN cnc.commission IS NULL THEN CAST(0.15 AS DECIMAL(18,2)) ELSE cnc.commission END 'commission',
		n.*
	FROM 
		cmw_traffic ct (NOLOCK) 
		JOIN networks n (NOLOCK) ON n.id=ct.network_id
		LEFT JOIN cmw_network_commissions cnc (NOLOCK) ON cnc.network_id=n.id
	ORDER BY 
		n.code
END
