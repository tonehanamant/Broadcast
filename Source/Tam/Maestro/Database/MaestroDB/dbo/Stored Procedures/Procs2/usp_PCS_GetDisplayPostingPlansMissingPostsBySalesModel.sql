-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 6/1/2012
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetDisplayPostingPlansMissingPostsBySalesModel]
	@sales_model_id INT
AS
BEGIN
	SELECT 
		dp.*
	FROM 
		uvw_display_proposals dp
	WHERE 
		dp.proposal_status_id=7
		AND dp.id NOT IN (
			SELECT DISTINCT posting_plan_proposal_id FROM tam_post_proposals tpp (NOLOCK)
		)
		AND dp.start_date>='12/26/2011'
		AND dp.sales_model_id IN (SELECT id FROM dbo.udf_GetSalesModelsFromSalesModel(@sales_model_id))
END
