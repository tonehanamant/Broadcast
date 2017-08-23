-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 1/23/2013
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetExcelNetworkMapsByDate]
	@effective_date DATETIME
AS
BEGIN
	SELECT
		nm.network_map_id,
		nm.network_id,
		nm.map_set,
		nm.map_value,
		nm.active,
		nm.flag,
		nm.start_date
	FROM
		uvw_network_maps nm (NOLOCK) 
	WHERE
		nm.map_set='excel' 
		AND nm.active=1
		AND nm.start_date<=@effective_date AND (nm.end_date>=@effective_date OR nm.end_date IS NULL)
	ORDER BY
		nm.map_value
END
