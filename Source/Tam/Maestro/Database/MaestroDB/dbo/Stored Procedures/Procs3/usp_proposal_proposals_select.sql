CREATE PROCEDURE usp_proposal_proposals_select
(
	@id Int
)
AS
SELECT
	id,
	parent_proposal_id,
	child_proposal_id,
	ordinal,
	cpm_percentage,
	rotation_percentage
FROM
	proposal_proposals (NOLOCK)
WHERE
	id = @id
