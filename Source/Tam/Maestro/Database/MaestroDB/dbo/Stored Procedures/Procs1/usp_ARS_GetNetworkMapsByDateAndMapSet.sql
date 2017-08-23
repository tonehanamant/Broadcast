-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 12/24/2012
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE usp_ARS_GetNetworkMapsByDateAndMapSet
	@network_map_set VARCHAR(31),
	@effective_date DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		nm.network_map_id,
		nm.start_date,
		nm.network_id,
		nm.map_set,
		nm.map_value,
		nm.active,
		nm.flag,
		CASE WHEN nm.end_date IS NULL THEN '12/31/9999' ELSE nm.end_date END 'end_date'
	FROM
		dbo.uvw_network_maps nm
	WHERE
		nm.map_set=@network_map_set
		AND (nm.start_date<=@effective_date AND (nm.end_date>=@effective_date OR nm.end_date IS NULL))
END
