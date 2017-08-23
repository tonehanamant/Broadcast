CREATE PROCEDURE [dbo].[usp_broadcast_proposal_detail_audiences_select]
(
	@broadcast_proposal_detail_id		Int,
	@audience_id		Int
)
AS
SELECT
	*
FROM
	broadcast_proposal_detail_audiences WITH(NOLOCK)
WHERE
	broadcast_proposal_detail_id=@broadcast_proposal_detail_id
	AND
	audience_id=@audience_id

