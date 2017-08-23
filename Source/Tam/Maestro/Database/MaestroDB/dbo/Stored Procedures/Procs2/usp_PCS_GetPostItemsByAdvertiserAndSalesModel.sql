-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 5/20/2010
-- Description:	<Description,,>
-- =============================================
-- usp_PCS_GetPostItemsByAdvertiserAndSalesModel 43201,1
CREATE PROCEDURE [dbo].[usp_PCS_GetPostItemsByAdvertiserAndSalesModel]
	@advertiser_company_id INT,
	@sales_model_id INT
AS
BEGIN
	SELECT DISTINCT
		tp.id,
		tp.title
	FROM
		tam_post_proposals tpp (NOLOCK)
		JOIN tam_posts tp (NOLOCK) ON tp.id = tpp.tam_post_id
			AND tp.is_deleted=0
		JOIN proposals posting_plan (NOLOCK) ON posting_plan.id = tpp.posting_plan_proposal_id
		JOIN proposals ordered_plan (NOLOCK) ON ordered_plan.id = posting_plan.original_proposal_id
		JOIN proposal_sales_models psm (NOLOCK) ON psm.proposal_id=ordered_plan.id
			AND psm.sales_model_id IN (SELECT id FROM dbo.udf_GetSalesModelsFromSalesModel(@sales_model_id))
	WHERE
		ordered_plan.advertiser_company_id = @advertiser_company_id
	ORDER BY
		tp.title
END
