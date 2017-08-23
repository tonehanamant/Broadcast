CREATE PROCEDURE usp_traffic_proposals_select
(
	@traffic_id		Int,
	@proposal_id		Int
)
AS
SELECT
	*
FROM
	traffic_proposals WITH(NOLOCK)
WHERE
	traffic_id=@traffic_id
	AND
	proposal_id=@proposal_id

