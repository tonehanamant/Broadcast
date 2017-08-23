CREATE PROCEDURE usp_proposal_audiences_select
(
	@proposal_id		Int,
	@audience_id		Int
)
AS
SELECT
	*
FROM
	proposal_audiences WITH(NOLOCK)
WHERE
	proposal_id=@proposal_id
	AND
	audience_id=@audience_id

