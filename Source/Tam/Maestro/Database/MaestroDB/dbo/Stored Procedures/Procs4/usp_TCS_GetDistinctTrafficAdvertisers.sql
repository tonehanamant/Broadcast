
CREATE PROCEDURE [dbo].[usp_TCS_GetDistinctTrafficAdvertisers]
AS
	SELECT DISTINCT
		proposals.advertiser_company_id
	FROM 
		proposals (NOLOCK)
		JOIN traffic_proposals (NOLOCK) on traffic_proposals.proposal_id = proposals.id 
		JOIN traffic (NOLOCK) on traffic_proposals.traffic_id = traffic.id
		JOIN proposal_sales_models psm with (NOLOCK) on psm.proposal_id = proposals.id
	WHERE
		psm.sales_model_id <> 4 
	ORDER BY
		proposals.advertiser_company_id
