-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_GetNetworkSubstitutions]
	@effective_date DATETIME
AS
BEGIN
	-- regional sports nets
	SELECT 
		nm.network_id, 
		CAST(nm.map_value AS INT) [substitute_network_id] 
	FROM 
		uvw_networkmap_universe nm (NOLOCK) 
	WHERE 
		nm.map_set='PostReplace' 
		AND active=1 
		AND (nm.start_date<=@effective_date AND (nm.end_date>=@effective_date OR nm.end_date IS NULL))

	UNION

	-- daypart networks (night to day)
	SELECT 
		nm.network_id, 
		CAST(nm.map_value AS INT) [substitute_network_id] 
	FROM 
		uvw_networkmap_universe nm (NOLOCK) 
	WHERE 
		nm.map_set='DaypartNetworks' 
		AND nm.flag=3
		AND active=1 
		AND (nm.start_date<=@effective_date AND (nm.end_date>=@effective_date OR nm.end_date IS NULL))
END
