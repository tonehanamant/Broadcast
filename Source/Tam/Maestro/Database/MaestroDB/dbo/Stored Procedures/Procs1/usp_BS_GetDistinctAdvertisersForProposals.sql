
CREATE PROCEDURE [dbo].[usp_BS_GetDistinctAdvertisersForProposals] 
AS
	SELECT 
		DISTINCT
		bp.advertiser_company_id
	FROM
		broadcast_proposals bp with (NOLOCK)
	ORDER BY
		bp.advertiser_company_id
