
-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 11/18/2009
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].usp_RCS_GetCoverageUniverseBaseMediaMonths
	@sales_model_id INT
AS
BEGIN
	SELECT
		*
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

