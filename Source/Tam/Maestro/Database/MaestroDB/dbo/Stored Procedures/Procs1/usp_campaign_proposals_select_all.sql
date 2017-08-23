CREATE PROCEDURE usp_campaign_proposals_select_all
AS
SELECT
	*
FROM
	campaign_proposals WITH(NOLOCK)
