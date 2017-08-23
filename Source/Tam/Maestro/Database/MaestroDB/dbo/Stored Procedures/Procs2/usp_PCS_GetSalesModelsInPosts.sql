-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 10/9/2013
-- Description:	<Description,,>
-- =============================================
-- usp_PCS_GetSalesModelsInPosts '1/10/2011'
CREATE PROCEDURE [dbo].[usp_PCS_GetSalesModelsInPosts]
	@effective_date DATETIME
AS
BEGIN
	SELECT DISTINCT
		sm.*
	FROM
		dbo.proposal_sales_models psm (NOLOCK)
		JOIN dbo.proposals p (NOLOCK) ON p.id=psm.proposal_id
		JOIN dbo.media_months mm (NOLOCK) ON mm.id=p.posting_media_month_id
			AND @effective_date BETWEEN mm.start_date AND mm.end_date
		JOIN dbo.sales_models sm (NOLOCK) ON sm.id=psm.sales_model_id
END
