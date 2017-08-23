
-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 4/7/2010
-- Description:	
-- =============================================
-- usp_ACS_GetTrafficMaterialInfoByMediaMonth 356
CREATE PROCEDURE [dbo].[usp_ACS_GetTrafficMaterialInfoByMediaMonth]
	@media_month_id INT
AS
BEGIN
	SELECT
		tm.traffic_id, 
		rm.material_id,
		m.product_id,
		tm.start_date,
		tm.end_date
	FROM
		traffic_materials tm	WITH(NOLOCK)
		JOIN reel_materials rm	WITH(NOLOCK) ON rm.id=tm.reel_material_id
		JOIN materials m		WITH(NOLOCK) ON m.id=rm.material_id
		JOIN traffic t			WITH(NOLOCK) ON t.id=tm.traffic_id
		JOIN media_months mm	WITH(NOLOCK) ON mm.id=@media_month_id
			AND mm.start_date <= tm.end_date AND tm.start_date <= mm.end_date
	WHERE
		t.release_id IS NOT NULL
END

