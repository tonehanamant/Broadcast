
CREATE PROCEDURE [dbo].[usp_PTS_GetProposalsWithTrafficByAdvertiser]
(
	@advertiser_id Int
)

AS
 
SELECT 
	proposals.id, 
	proposals.name 
FROM 
	proposals (NOLOCK) 
where 
	proposals.advertiser_company_id = @advertiser_id 
	and proposals.id in 
	(SELECT distinct proposal_id from traffic_proposals (NOLOCK))
ORDER BY 
	proposals.id, 
	proposals.name
