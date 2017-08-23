-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
-- usp_PCS_GetNetworkMapSetByDate 'PostReplace', '10/5/2009'
CREATE PROCEDURE [dbo].[usp_PCS_GetNetworkMapSetByDate]
	@map_set VARCHAR(15),
	@effective_date DATETIME
AS
BEGIN
	SELECT
		network_map_id,
		network_id,
		map_set,
		map_value,
		active,
		flag,
		start_date
	FROM
		uvw_networkmap_universe nm (NOLOCK)
	WHERE
		map_set=@map_set
		AND (nm.start_date<=@effective_date AND (nm.end_date>=@effective_date OR nm.end_date IS NULL))
END
