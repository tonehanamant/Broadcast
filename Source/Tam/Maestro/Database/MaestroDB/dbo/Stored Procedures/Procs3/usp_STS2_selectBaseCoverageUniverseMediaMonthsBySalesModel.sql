-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 5/1/2014
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_selectBaseCoverageUniverseMediaMonthsBySalesModel]
	@sales_model_id INT
AS
BEGIN
	SELECT
		mm.*
	FROM
		media_months mm (NOLOCK)
	WHERE
		mm.id IN (
			SELECT 
				base_media_month_id 
			FROM
				coverage_universes cu (NOLOCK)
			WHERE
				cu.date_approved IS NOT NULL
				AND sales_model_id=@sales_model_id
		)
	ORDER BY
		start_date DESC
END
