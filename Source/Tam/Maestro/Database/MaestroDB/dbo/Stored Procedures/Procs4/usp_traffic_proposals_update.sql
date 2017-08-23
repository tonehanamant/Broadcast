CREATE PROCEDURE usp_traffic_proposals_update
(
	@traffic_id		Int,
	@proposal_id		Int,
	@primary_proposal		Bit
)
AS
UPDATE traffic_proposals SET
	primary_proposal = @primary_proposal
WHERE
	traffic_id = @traffic_id AND
	proposal_id = @proposal_id
