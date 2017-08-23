/****** Object:  Table [dbo].[usp_TCS_GetProposalsWithTrafficByAdvertiser]    Script Date: 11/16/2012 14:51:25 ******/
CREATE PROCEDURE [dbo].[usp_TCS_GetProposalsWithTrafficByAdvertiser]
(
	@advertiser_id Int
)
AS
	SELECT 
		proposals.id, 
		proposals.name 
	FROM 
		proposals (NOLOCK) 
		JOIN proposal_sales_models psm (NOLOCK) on psm.proposal_id = proposals.id
	WHERE 
		proposals.advertiser_company_id = @advertiser_id 
		AND psm.sales_model_id <> 4 
		AND proposals.id IN (
			SELECT DISTINCT proposal_id FROM traffic_proposals (NOLOCK)
		)
	ORDER BY 
		proposals.id, 
		proposals.name
