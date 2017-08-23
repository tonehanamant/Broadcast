
CREATE PROCEDURE [dbo].[usp_PCS_GetPostedAdvertiserItemsBySalesModel]
	@sales_model_id INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT DISTINCT
		ordered_plan.advertiser_company_id
	FROM
		tam_post_proposals tpp (NOLOCK)
		JOIN proposals posting_plan (NOLOCK) ON posting_plan.id = tpp.posting_plan_proposal_id
		JOIN proposals ordered_plan (NOLOCK) ON ordered_plan.id = posting_plan.original_proposal_id
		JOIN proposal_sales_models psm (NOLOCK) ON psm.proposal_id=ordered_plan.id
			AND psm.sales_model_id IN (SELECT id FROM dbo.udf_GetSalesModelsFromSalesModel(@sales_model_id))
END
