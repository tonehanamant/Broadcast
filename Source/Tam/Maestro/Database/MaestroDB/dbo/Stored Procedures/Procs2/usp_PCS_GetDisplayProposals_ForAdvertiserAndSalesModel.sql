-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetDisplayProposals_ForAdvertiserAndSalesModel]
	@sales_model_id INT,
	@company_id INT
AS
BEGIN
    SELECT 
		dp.*
	FROM 
		uvw_display_proposals dp
		JOIN proposals p (NOLOCK) ON p.id=dp.id
			AND p.advertiser_company_id=@company_id
		JOIN proposal_sales_models psm (NOLOCK) ON psm.proposal_id=dp.id
			AND psm.sales_model_id IN (SELECT id FROM dbo.udf_GetSalesModelsFromSalesModel(@sales_model_id))
	WHERE
		dp.proposal_status_id<>7
	ORDER BY 
		dp.id DESC
END
