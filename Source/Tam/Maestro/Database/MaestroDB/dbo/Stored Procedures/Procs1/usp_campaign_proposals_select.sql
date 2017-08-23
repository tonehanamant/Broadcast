CREATE PROCEDURE usp_campaign_proposals_select
(
	@proposal_id		Int,
	@campaign_id		Int
)
AS
SELECT
	*
FROM
	campaign_proposals WITH(NOLOCK)
WHERE
	proposal_id=@proposal_id
	AND
	campaign_id=@campaign_id

