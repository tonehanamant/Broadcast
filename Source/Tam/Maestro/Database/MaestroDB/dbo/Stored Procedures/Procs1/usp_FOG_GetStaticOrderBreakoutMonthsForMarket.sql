
-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 9/23/2013
-- Description:	<Description,,>
-- =============================================
-- usp_FOG_GetStaticOrderBreakoutMonths 2797,104,135,385,48737,'9/30/2013'
CREATE PROCEDURE [dbo].[usp_FOG_GetStaticOrderBreakoutMonthsForMarket]
	@system_id INT,
	@topography_id INT,
	@dma_id INT,
	@media_month_id INT,
	@proposal_id INT,
	@effective_date DATETIME
AS
BEGIN
	SET NOCOUNT ON;
	
    SELECT
		mm.*,
		ROUND(SUM(so.ordered_rate * so.ordered_units),2) 'total_cost', 
		SUM(so.ordered_units) 'total_units'
	FROM
		static_orders so (NOLOCK)
		JOIN media_weeks mw (NOLOCK) ON mw.id=so.media_week_id
			AND mw.media_month_id=@media_month_id
		JOIN media_months mm (NOLOCK) ON mm.id=@media_month_id
		JOIN uvw_zonedma_universe zd ON zd.zone_id=so.zone_id
			AND (zd.start_date<=@effective_date AND (zd.end_date>=@effective_date OR zd.end_date IS NULL))
			AND (@dma_id IS NULL OR zd.dma_id=@dma_id)
	WHERE
		so.topography_id=@topography_id
		AND so.system_id=@system_id
		AND so.proposal_id=@proposal_id
	GROUP BY
		mm.id,
		mm.year,
		mm.month,
		mm.media_month,
		mm.start_date,
		mm.end_date
END
