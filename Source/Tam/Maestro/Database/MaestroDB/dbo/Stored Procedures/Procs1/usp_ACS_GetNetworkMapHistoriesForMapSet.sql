-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_ACS_GetNetworkMapHistoriesForMapSet]
	@map_set VARCHAR(15)
AS
BEGIN
	SELECT
		network_map_id,
		start_date,
		network_id,
		map_set,
		map_value,
		active,
		flag,
		end_date
	FROM
		network_map_histories (NOLOCK)
	WHERE
		map_set LIKE @map_set
END