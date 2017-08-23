CREATE PROCEDURE usp_traffic_proposals_insert
(
	@traffic_id		Int,
	@proposal_id		Int,
	@primary_proposal		Bit
)
AS
INSERT INTO traffic_proposals
(
	traffic_id,
	proposal_id,
	primary_proposal
)
VALUES
(
	@traffic_id,
	@proposal_id,
	@primary_proposal
)

