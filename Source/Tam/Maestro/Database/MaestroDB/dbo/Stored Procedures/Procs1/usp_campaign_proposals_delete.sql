CREATE PROCEDURE usp_campaign_proposals_delete
(
	@proposal_id		Int,
	@campaign_id		Int)
AS
DELETE FROM
	campaign_proposals
WHERE
	proposal_id = @proposal_id
 AND
	campaign_id = @campaign_id
