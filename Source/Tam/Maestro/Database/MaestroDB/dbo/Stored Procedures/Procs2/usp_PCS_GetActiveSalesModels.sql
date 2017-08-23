-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 4/28/2010
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetActiveSalesModels]
	@effective_date DATETIME
AS
BEGIN
	SELECT
		s.*
	FROM
		sales_models s (NOLOCK)
	WHERE
		id IN (
			SELECT 
				smap.sales_model_id 
			FROM 
				sales_model_active_periods smap (NOLOCK)
			WHERE
				(smap.[start_date]<=@effective_date AND (smap.end_date>=@effective_date OR smap.end_date IS NULL))
		)
	ORDER BY
		s.name
END
