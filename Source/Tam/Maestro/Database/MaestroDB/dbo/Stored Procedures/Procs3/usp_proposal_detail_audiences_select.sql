CREATE PROCEDURE usp_proposal_detail_audiences_select
(
	@proposal_detail_id		Int,
	@audience_id		Int
)
AS
SELECT
	*
FROM
	proposal_detail_audiences WITH(NOLOCK)
WHERE
	proposal_detail_id=@proposal_detail_id
	AND
	audience_id=@audience_id

