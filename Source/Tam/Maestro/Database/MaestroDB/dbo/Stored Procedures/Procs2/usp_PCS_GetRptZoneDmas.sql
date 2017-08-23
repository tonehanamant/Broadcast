-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 10/27/2011
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetRptZoneDmas]
AS
BEGIN
	SELECT 
		zd.zone_id,
		zd.dma_id,
		zd.start_date,
		zd.weight,
		zd.end_date
	FROM 
		uvw_rptzonedma_universe zd
	ORDER BY
		zd.zone_id,
		zd.start_date
END
