

-- usp_REL_GetProposalAudiencesForExportDetails 22598
CREATE Procedure [dbo].[usp_REL_GetProposalAudiencesForExportDetails]
	@proposal_detail_id Int
AS
 
SELECT 
	proposal_audiences.audience_id, 
	proposal_audiences.universe, 
	proposal_detail_audiences.us_universe, 
	proposal_detail_audiences.rating
FROM
	proposal_detail_audiences	(NOLOCK)
	JOIN proposal_audiences		(NOLOCK) ON proposal_audiences.audience_id=proposal_detail_audiences.audience_id
WHERE 
	proposal_detail_audiences.proposal_detail_id=@proposal_detail_id
	AND proposal_audiences.proposal_id IN (
		SELECT proposal_id FROM proposal_details WHERE id=@proposal_detail_id
	)
ORDER BY
	proposal_audiences.ordinal

