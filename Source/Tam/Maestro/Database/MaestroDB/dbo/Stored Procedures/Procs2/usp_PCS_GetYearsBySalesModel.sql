-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetYearsBySalesModel]
	@sales_model_id INT
AS
BEGIN
	SELECT
		DISTINCT mm.[year]
	FROM
		proposals p (NOLOCK)
		JOIN proposal_sales_models psm (NOLOCK) ON psm.proposal_id=p.id
			AND psm.sales_model_id IN (SELECT id FROM dbo.udf_GetSalesModelsFromSalesModel(@sales_model_id))
		JOIN media_months mm (NOLOCK) ON (mm.start_date <= p.end_date AND mm.end_date >= p.start_date)
	ORDER BY
		mm.[year] DESC
END
