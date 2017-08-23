
CREATE PROCEDURE [dbo].[usp_TCS_GetPrimaryProposal]
(@id as int)
AS
	SELECT 
		distinct traffic_proposals.proposal_id 
	FROM 
		traffic_proposals (NOLOCK) 
	where
		traffic_proposals.traffic_id = @id and traffic_proposals.primary_proposal = 1
