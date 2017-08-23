CREATE PROCEDURE usp_proposal_proposals_update
(
	@id		Int,
	@parent_proposal_id		Int,
	@child_proposal_id		Int,
	@ordinal		Int,
	@cpm_percentage		Int,
	@rotation_percentage		Int
)
AS
UPDATE proposal_proposals SET
	parent_proposal_id = @parent_proposal_id,
	child_proposal_id = @child_proposal_id,
	ordinal = @ordinal,
	cpm_percentage = @cpm_percentage,
	rotation_percentage = @rotation_percentage
WHERE
	id = @id

