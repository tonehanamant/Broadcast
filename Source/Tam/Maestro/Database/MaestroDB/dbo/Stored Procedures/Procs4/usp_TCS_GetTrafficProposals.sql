

CREATE PROCEDURE [dbo].[usp_TCS_GetTrafficProposals]
(
@id as int
)
AS

SELECT 
	traffic_proposals.traffic_id, 
	traffic_proposals.proposal_id, 
	traffic_proposals.primary_proposal
from 
	traffic_proposals (NOLOCK)
where 
	traffic_proposals.traffic_id = @id

