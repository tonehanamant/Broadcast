-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetExcelNetworkMaps]
AS
BEGIN
	SELECT
		id, 
		network_id, 
		map_set, 
		map_value, 
		active, 
		flag, 
		effective_date
	FROM
		network_maps (NOLOCK) 
	WHERE
		map_set='excel' 
		AND active=1
	ORDER BY
		map_value
END
