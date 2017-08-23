-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 
-- Description:	Gets the laetst base media month for coverage universes by sales model.
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_selectLatestBaseCoverageUniverseMediaMonth]
	@sales_model_id INT
AS
BEGIN
	SET NOCOUNT ON;

    SELECT 
		mm.*
	FROM 
		dbo.media_months mm (NOLOCK)
	WHERE
		mm.id=(
			SELECT MAX(base_media_month_id) FROM coverage_universes (NOLOCK) WHERE date_approved IS NOT NULL AND sales_model_id=@sales_model_id
		)
END
