-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 3/2/2011
-- Description:	Retrieves a table of zone_dma relationships specifically for the legacy TRP/GRP report.
-- =============================================
CREATE FUNCTION [dbo].[udf_GetZoneDmasForTrpGrpReport]
(	
	@effective_date DATETIME
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT 
		zd.zone_id,
		zd.dma_id,
		zd.weight,
		zd.start_date,
		zd.end_date
	FROM 
		uvw_zonedma_universe zd
	WHERE 
		zd.zone_id NOT IN (
			SELECT DISTINCT
				zone_id
			FROM
				uvw_rptzonedma_universe
			WHERE
				(start_date<=@effective_date AND (end_date>=@effective_date OR end_date IS NULL))
		)
		AND (zd.start_date<=@effective_date AND (zd.end_date>=@effective_date OR zd.end_date IS NULL))

	UNION ALL

	SELECT 
		zd.zone_id,
		zd.dma_id,
		zd.weight,
		zd.start_date,
		zd.end_date
	FROM 
		uvw_rptzonedma_universe zd
	WHERE
		(zd.start_date<=@effective_date AND (zd.end_date>=@effective_date OR zd.end_date IS NULL))
)
