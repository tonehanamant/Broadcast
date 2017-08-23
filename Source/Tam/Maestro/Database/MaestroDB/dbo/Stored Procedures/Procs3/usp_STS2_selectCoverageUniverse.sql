-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_selectCoverageUniverse]
	@topography_id INT,
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
		cud.topography_id=@topography_id
		AND cu.base_media_month_id=@base_media_month_id
		AND cu.sales_model_id=@sales_model_id
	GROUP BY
		cud.network_id
END
