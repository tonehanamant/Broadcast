-- =============================================
-- Author:		Stephen DeFusco
-- Create date:	9/26/2013
-- Description:	<Description,,>
-- =============================================
-- usp_FOG_GetDisplayDmasBySystem 2798, '9/30/2013'
CREATE Procedure [dbo].[usp_FOG_GetDisplayDmasBySystem]
	@system_id INT,
	@effective_date DATETIME
AS
BEGIN
	CREATE TABLE #tmp (zone_id INT, dma_id INT)
	INSERT INTO #tmp
		SELECT DISTINCT 
			si.zone_id,
			zd.dma_id
		FROM 
			static_inventories si (NOLOCK)
			JOIN uvw_zonedma_universe zd ON zd.zone_id=si.zone_id
				AND (zd.start_date<=@effective_date AND (zd.end_date>=@effective_date OR zd.end_date IS NULL))
		WHERE
			si.system_id=@system_id
		
	SELECT
		*
	FROM
		#tmp;
			
	SELECT
		d.dma_id,
		d.code,
		ISNULL(dm.map_value, d.name) 'name',
		d.rank,
		d.tv_hh,
		d.cable_hh,
		d.active,
		d.start_date,
		d.flag,
		d.end_date,
		dbo.GetSubscribersForDma(d.dma_id,@effective_date,1,1) 'subscribers'
	FROM 
		uvw_dma_universe d (NOLOCK)
		LEFT JOIN dma_maps dm (NOLOCK) ON dm.dma_id=d.dma_id 
			AND dm.map_set = 'Strata'
	WHERE 
		d.dma_id IN (
			SELECT DISTINCT dma_id FROM #tmp
		)
		AND (d.start_date<=@effective_date AND (d.end_date>=@effective_date OR d.end_date IS NULL))
		
	DROP TABLE #tmp;
END