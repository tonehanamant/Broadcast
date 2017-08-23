CREATE PROCEDURE usp_proposal_proposals_insert
(
	@id		int		OUTPUT,
	@parent_proposal_id		Int,
	@child_proposal_id		Int,
	@ordinal		Int,
	@cpm_percentage		Int,
	@rotation_percentage		Int
)
AS
INSERT INTO proposal_proposals
(
	parent_proposal_id,
	child_proposal_id,
	ordinal,
	cpm_percentage,
	rotation_percentage
)
VALUES
(
	@parent_proposal_id,
	@child_proposal_id,
	@ordinal,
	@cpm_percentage,
	@rotation_percentage
)

SELECT
	@id = SCOPE_IDENTITY()

