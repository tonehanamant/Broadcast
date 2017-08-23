-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 7/23/2009
-- Description:	
-- =============================================
-- usp_STS2_selectCoverageUniverses '1,9,18,20,16,21',69,2
CREATE PROCEDURE [dbo].[usp_STS2_selectCoverageUniverses]
	@topography_ids VARCHAR(MAX),
	@base_media_month_id INT,
	@sales_model_id INT
AS
BEGIN
	SELECT 
		cud.network_id,
		SUM(cud.universe) 
	FROM 
		coverage_universe_details	cud (NOLOCK)
		JOIN coverage_universes		cu	(NOLOCK) ON cu.id=cud.coverage_universe_id
	WHERE
		cud.topography_id IN (
			SELECT id FROM dbo.SplitIntegers(@topography_ids)
		)
		AND cu.base_media_month_id=@base_media_month_id
		AND cu.sales_model_id=@sales_model_id
	GROUP BY
		cud.network_id
END
