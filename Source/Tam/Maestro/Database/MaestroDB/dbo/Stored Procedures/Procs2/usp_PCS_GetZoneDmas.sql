-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 11/2/2011
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetZoneDmas]
AS
BEGIN
	-- zones to exclude b/c their weights dont add up to one
	CREATE TABLE #zones_to_exclude (zone_id INT)
	INSERT INTO #zones_to_exclude
		SELECT
			zone_id
		FROM (
			SELECT
				zone_id,
				CAST(ROUND(SUM(weight),1) AS DECIMAL(18,1)) 'weight'
			FROM
				rpt_zone_dmas
			GROUP BY
				zone_id
			HAVING
				CAST(ROUND(SUM(weight),1) AS DECIMAL(18,1)) <> 1.0
		) tmp
		
	SELECT 
		zd.zone_id,
		zd.dma_id,
		zd.start_date,
		zd.weight,
		zd.end_date
	FROM 
		uvw_rptzonedma_universe zd
	WHERE
		zd.zone_id NOT IN (
			SELECT zone_id FROM #zones_to_exclude
		)
		
	UNION ALL
	
	SELECT
		zd.zone_id,
		zd.dma_id,
		zd.start_date,
		zd.weight,
		zd.end_date
	FROM
		uvw_zonedma_universe zd
		JOIN #zones_to_exclude zte ON zte.zone_id=zd.zone_id
		
	DROP TABLE #zones_to_exclude;
END
