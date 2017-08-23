
-- =============================================
-- Date       Author			Description   
-- ---------  ------			------------------------------------
-- 7/24/2009 Stephen DeFusco   Created new procedure
-- 09/18/2015 Abdul Sukkur	   Updated frozen universe table
-- =============================================
CREATE PROCEDURE [dbo].[usp_RCS_GetAvailableCoverageUniverseBaseMediaMonths]
	@sales_model_id INT
AS
BEGIN
	SELECT
		distinct mm.*
	FROM
		media_months mm (NOLOCK)
	INNER JOIN 
		frozen_media_months fmm (NOLOCK) on mm.id = fmm.media_month_id	
	WHERE
		mm.id NOT IN (
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
