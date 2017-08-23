CREATE PROCEDURE usp_campaign_proposals_insert
(
	@proposal_id		Int,
	@campaign_id		Int
)
AS
INSERT INTO campaign_proposals
(
	proposal_id,
	campaign_id
)
VALUES
(
	@proposal_id,
	@campaign_id
)

