-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 9/10/2010
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetPostedYearsBySalesModel]
	@sales_model_id INT
AS
BEGIN
	SELECT
		DISTINCT mm.[year]
	FROM
		tam_post_proposals tpp (NOLOCK)
		JOIN proposals p (NOLOCK) ON p.id = tpp.posting_plan_proposal_id
		JOIN proposal_sales_models psm (NOLOCK) ON psm.proposal_id=p.id
			AND psm.sales_model_id IN (SELECT id FROM dbo.udf_GetSalesModelsFromSalesModel(@sales_model_id))
		JOIN media_months mm (NOLOCK) ON mm.id=p.posting_media_month_id
	ORDER BY
		mm.[year] DESC
END
