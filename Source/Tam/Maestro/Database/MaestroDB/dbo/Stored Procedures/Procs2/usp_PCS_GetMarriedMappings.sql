CREATE Procedure [dbo].[usp_PCS_GetMarriedMappings]
	@proposal_id int
AS

SELECT 
	id,
	parent_proposal_id,
	child_proposal_id,
	ordinal,
	cpm_percentage,
	rotation_percentage
FROM
	proposal_proposals WITH (NOLOCK)
WHERE
	proposal_proposals.parent_proposal_id = @proposal_id
ORDER BY
	proposal_proposals.ordinal

