CREATE PROCEDURE usp_proposal_detail_worksheets_select
(
	@proposal_detail_id		Int,
	@media_week_id		Int
)
AS
SELECT
	*
FROM
	proposal_detail_worksheets WITH(NOLOCK)
WHERE
	proposal_detail_id=@proposal_detail_id
	AND
	media_week_id=@media_week_id

