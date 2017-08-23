
-- usp_PCS_GetDisplayAdvertiserPostsBySalesModel
CREATE PROCEDURE [dbo].[usp_PCS_GetDisplayAdvertiserPostsBySalesModel]
	@sales_model_id INT
AS
BEGIN
	SELECT DISTINCT
		id,
		COUNT(*) 'total_posts'
	FROM
	(
		SELECT DISTINCT
			ordered_plan.advertiser_company_id as id,
			tpp.tam_post_id
		FROM
			tam_post_proposals tpp (NOLOCK)
			JOIN proposals posting_plan (NOLOCK) ON posting_plan.id = tpp.posting_plan_proposal_id
			JOIN proposals ordered_plan (NOLOCK) ON ordered_plan.id = posting_plan.original_proposal_id
			JOIN proposal_sales_models psm (NOLOCK) ON psm.proposal_id=ordered_plan.id
				AND psm.sales_model_id IN (SELECT id FROM dbo.udf_GetSalesModelsFromSalesModel(@sales_model_id))
	) tmp
	GROUP BY
		id
END
