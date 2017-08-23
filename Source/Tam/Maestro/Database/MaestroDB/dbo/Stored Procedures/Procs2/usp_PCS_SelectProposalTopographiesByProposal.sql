CREATE PROCEDURE [dbo].[usp_PCS_SelectProposalTopographiesByProposal]
(
	@proposal_id		Int
)
AS
SELECT
	topography_id,
	proposal_id
FROM
	proposal_topographies (NOLOCK) 
WHERE
	proposal_id=@proposal_id
